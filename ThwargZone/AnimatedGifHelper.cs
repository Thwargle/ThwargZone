using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ThwargZone
{
    /// <summary>
    /// Helper class to enable animated GIF support in WPF Image controls
    /// </summary>
    public static class ImageBehavior
    {
        public static readonly DependencyProperty AnimatedSourceProperty =
            DependencyProperty.RegisterAttached(
                "AnimatedSource",
                typeof(BitmapImage),
                typeof(ImageBehavior),
                new PropertyMetadata(null, OnAnimatedSourceChanged));

        public static BitmapImage GetAnimatedSource(System.Windows.Controls.Image image)
        {
            return (BitmapImage)image.GetValue(AnimatedSourceProperty);
        }

        public static void SetAnimatedSource(System.Windows.Controls.Image image, BitmapImage value)
        {
            image.SetValue(AnimatedSourceProperty, value);
        }

        private static void OnAnimatedSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.Image image && e.NewValue is BitmapImage bitmapImage)
            {
                try
                {
                    // Load the GIF from the URI
                    var uri = bitmapImage.UriSource;
                    if (uri != null)
                    {
                        // Convert pack URI to stream
                        var streamInfo = Application.GetResourceStream(uri);
                        if (streamInfo != null)
                        {
                            // Copy to MemoryStream to keep it alive
                            var memoryStream = new MemoryStream();
                            streamInfo.Stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            
                            var gifImage = System.Drawing.Image.FromStream(memoryStream);
                            AnimateGif(image, gifImage, memoryStream);
                        }
                    }
                }
                catch
                {
                    // If animation fails, just use the regular image
                    image.Source = bitmapImage;
                }
            }
        }

        private static void AnimateGif(System.Windows.Controls.Image image, System.Drawing.Image gifImage, MemoryStream memoryStream)
        {
            if (!ImageAnimator.CanAnimate(gifImage))
            {
                // Not an animated GIF, use regular display
                image.Source = ConvertToWpfBitmap(gifImage);
                memoryStream.Dispose();
                return;
            }

            // Get the number of frames in the GIF
            var dimension = new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]);
            int frameCount = gifImage.GetFrameCount(dimension);
            
            // Get frame delays from GIF metadata (in 1/100ths of a second)
            var item = gifImage.GetPropertyItem(0x5100); // PropertyTagFrameDelay
            int[] delays = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                // Each delay is 4 bytes
                delays[i] = BitConverter.ToInt32(item.Value, i * 4) * 10; // Convert to milliseconds
                
                // Use the GIF's own timing, with a minimum of 10ms to prevent too-fast playback
                if (delays[i] < 10) delays[i] = 10;
            }

            int currentFrame = 0;
            var timer = new DispatcherTimer();
            
            EventHandler onFrameChanged = null;
            onFrameChanged = (s, e) =>
            {
                try
                {
                    if (image.Dispatcher.CheckAccess())
                    {
                        // Select the current frame
                        gifImage.SelectActiveFrame(dimension, currentFrame);
                        image.Source = ConvertToWpfBitmap(gifImage);
                        
                        // Set the delay for the next frame
                        timer.Interval = TimeSpan.FromMilliseconds(delays[currentFrame]);
                        
                        // Move to next frame (loop back to 0 after last frame)
                        currentFrame = (currentFrame + 1) % frameCount;
                    }
                    else
                    {
                        image.Dispatcher.Invoke(() =>
                        {
                            gifImage.SelectActiveFrame(dimension, currentFrame);
                            image.Source = ConvertToWpfBitmap(gifImage);
                            timer.Interval = TimeSpan.FromMilliseconds(delays[currentFrame]);
                            currentFrame = (currentFrame + 1) % frameCount;
                        });
                    }
                }
                catch
                {
                    // If there's an error, stop the timer
                    timer.Stop();
                }
            };

            timer.Tick += onFrameChanged;
            timer.Interval = TimeSpan.FromMilliseconds(delays[0]);
            timer.Start();

            // Display first frame immediately
            image.Source = ConvertToWpfBitmap(gifImage);

            image.Unloaded += (s, e) =>
            {
                timer.Stop();
                timer.Tick -= onFrameChanged;
                gifImage.Dispose();
                memoryStream.Dispose();
            };
        }

        private static BitmapSource ConvertToWpfBitmap(System.Drawing.Image gdiImage)
        {
            using (var memory = new MemoryStream())
            {
                gdiImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}

