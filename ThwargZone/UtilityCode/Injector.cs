using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace ThwargZone.Win32
{
    static class Injector
    {
        /// <summary>
        /// This will start an application using Process.Start() and inject the dll.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool RunWithInject(ProcessStartInfo processStartInfo, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            var process = Process.Start(processStartInfo);

            if (process == null || process.Handle == IntPtr.Zero)
                return false;

            return Inject(process, pathOfDllToInject, dllFunctionToExecute);
        }

        /// <summary>
        /// This will start an application using kernel32.CreateProcess() suspended, inject the dll and then resume the process.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected and before the process is resumed.
        /// </summary>
        /// <remarks>
        /// This function was cleverly named by parad0x, one of the developers of Decal for Asheron's Call.
        /// </remarks>
        public static bool RunSuspendedCommaInjectCommaAndResume(string fileName, string arguments, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            try
            {
                // Verify files exist
                if (!File.Exists(fileName) || !File.Exists(pathOfDllToInject))
                    return false;
                
                // Reference: https://docs.microsoft.com/en-us/windows/desktop/procthread/process-creation-flags
                const uint CREATE_SUSPENDED = 0x00000004;

                kernel32.SECURITY_ATTRIBUTES pSec = new kernel32.SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                kernel32.SECURITY_ATTRIBUTES tSec = new kernel32.SECURITY_ATTRIBUTES();
                tSec.nLength = Marshal.SizeOf(tSec);
                kernel32.STARTUPINFO sInfo = new kernel32.STARTUPINFO();

                string commandLine = fileName + " " + arguments;
                
                if (!kernel32.CreateProcess(null, commandLine, ref pSec, ref tSec, false, CREATE_SUSPENDED, IntPtr.Zero, Path.GetDirectoryName(fileName), ref sInfo, out var pInfo))
                    return false;

                try
                {
                    return Inject(pInfo.hProcess, pathOfDllToInject, dllFunctionToExecute);
                }
                finally
                {
                    kernel32.ResumeThread(pInfo.hThread);
                    kernel32.CloseHandle(pInfo.hThread);
                    kernel32.CloseHandle(pInfo.hProcess);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by applicationName.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(string applicationName, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by process.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(Process process, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            return Inject(process.Handle, pathOfDllToInject, dllFunctionToExecute);
        }

        /// <summary>
        /// This will inject a dll into an existing process defined by processHandle.<para />
        /// If dllFunctionToExecute is defined, it will be called after the dll has been injected.
        /// </summary>
        public static bool Inject(IntPtr processHandle, string pathOfDllToInject, string dllFunctionToExecute = null)
        {
            try
            {
                // Allocating some memory on the target process - enough to store the name of the dll and storing its address in a pointer
                var dwSize = (uint)((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char)));

                var allocMemAddress = kernel32.VirtualAllocEx(processHandle, IntPtr.Zero, dwSize, kernel32.AllocationType.Commit | kernel32.AllocationType.Reserve, kernel32.MemoryProtection.ReadWrite);

                if (allocMemAddress == IntPtr.Zero)
                    return false;

                try
                {
                    // Writing the name of the dll there
                    if (!kernel32.WriteProcessMemory(processHandle, allocMemAddress, Encoding.Default.GetBytes(pathOfDllToInject), (uint)((pathOfDllToInject.Length + 1) * Marshal.SizeOf(typeof(char))), out _))
                        return false;

                    // Searching for the address of LoadLibraryA and storing it in a pointer
                    var kernel32Handle = kernel32.GetModuleHandle("kernel32.dll");
                    var loadLibraryAddr = kernel32.GetProcAddress(kernel32Handle, "LoadLibraryA");
                    
                    if (loadLibraryAddr == IntPtr.Zero)
                        return false;

                    // Inject the DLL
                    var remoteThreadHandle = kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                    if (remoteThreadHandle == IntPtr.Zero)
                        return false;

                    try
                    {
                        kernel32.WaitForSingleObject(remoteThreadHandle, kernel32.INFINITE);

                        kernel32.GetExitCodeThread(remoteThreadHandle, out var injectedDllAddress);

                        if (injectedDllAddress != 0)
                        {
                            // If we have a function to execute, lets do it
                            if (!String.IsNullOrEmpty(dllFunctionToExecute))
                                return Execute(processHandle, injectedDllAddress, pathOfDllToInject, dllFunctionToExecute);

                            return true;
                        }

                        return false;
                    }
                    finally
                    {
                        kernel32.CloseHandle(remoteThreadHandle);
                    }
                }
                finally
                {
                    kernel32.VirtualFreeEx(processHandle, allocMemAddress, dwSize, kernel32.AllocationType.Release);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This will find the address offset of dllFunctionToExecute in pathOfDllToInject, and add that to injectedDllAddress;
        /// It will then use kernel32.CreateRemoteThread() to call that address on processHandle.
        /// </summary>
        public static bool Execute(IntPtr processHandle, uint injectedDllAddress, string pathOfDllToInject, string dllFunctionToExecute)
        {
            var libraryAddress = kernel32.LoadLibrary(pathOfDllToInject);

            if (libraryAddress == IntPtr.Zero)
                return false;

            try
            {
                var functionAddress = kernel32.GetProcAddress(libraryAddress, dllFunctionToExecute);

                if (functionAddress == IntPtr.Zero)
                    return false;

                var functionAddressOffset = functionAddress.ToInt64() - libraryAddress.ToInt64();
                var addressToExecute = injectedDllAddress + functionAddressOffset;

                return Execute(processHandle, (IntPtr)addressToExecute);
            }
            finally
            {
                kernel32.FreeLibrary(libraryAddress);
            }
        }

        /// <summary>
        /// This will use kernel32.CreateRemoteThread() to call addressToExecute on processHandle.
        /// </summary>
        public static bool Execute(IntPtr processHandle, IntPtr addressToExecute)
        {
            var remoteThreadHandle = kernel32.CreateRemoteThread(processHandle, IntPtr.Zero, 0, addressToExecute, IntPtr.Zero, 0, IntPtr.Zero);

            if (remoteThreadHandle == IntPtr.Zero)
                return false;

            try
            {
                kernel32.WaitForSingleObject(remoteThreadHandle, kernel32.INFINITE);

                kernel32.GetExitCodeThread(remoteThreadHandle, out var exitCode);

                return exitCode != 0;
            }
            finally
            {
                kernel32.CloseHandle(remoteThreadHandle);
            }
        }
        
        /// <summary>
        /// Get Decal location from registry
        /// </summary>
        public static string GetDecalLocation()
        {
            string subKey = @"SOFTWARE\Decal\Agent";
            
            // Try 64-bit registry first, then 32-bit
            RegistryKey sk1 = TryOpenRegistryKey(subKey);
            
            if (sk1 == null)
            {
                throw new Exception("Decal registry key not found: " + subKey);
            }

            try
            {
                string decalAgentPath = (string)sk1.GetValue("AgentPath", "");
                if (string.IsNullOrEmpty(decalAgentPath)) 
                { 
                    throw new Exception("Decal AgentPath not found in registry"); 
                }

                // Use Inject.dll for injection (this is the actual injector)
                string injectDll = Path.Combine(decalAgentPath, "Inject.dll");
                if (File.Exists(injectDll))
                {
                    return injectDll;
                }

                throw new Exception($"Inject.dll not found in: {decalAgentPath}");
            }
            catch (Exception exc)
            {
                throw new Exception("No Decal in registry: " + exc.Message);
            }
            finally
            {
                sk1?.Close();
            }
        }
        
        /// <summary>
        /// Check if Decal is installed
        /// </summary>
        public static bool IsDecalInstalled()
        {
            string subKey = @"SOFTWARE\Decal\Agent";
            
            // Try 64-bit registry first, then 32-bit
            RegistryKey sk1 = TryOpenRegistryKey(subKey);
            
            if (sk1 == null) { return false; }

            try
            {
                string decalInjectionFile = (string)sk1.GetValue("AgentPath", "");
                if (string.IsNullOrEmpty(decalInjectionFile)) { return false; }
                decalInjectionFile += "Inject.dll";

                if (!File.Exists(decalInjectionFile)) { return false; }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                sk1?.Close();
            }
        }
        
        /// <summary>
        /// Try to open registry key with both 64-bit and 32-bit views
        /// </summary>
        private static RegistryKey TryOpenRegistryKey(string subKey)
        {
            try
            {
                // Try 64-bit registry first
                RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, false);
                if (key != null)
                {
                    return key;
                }
            }
            catch
            {
                // Ignore and try 32-bit
            }
            
            try
            {
                // Try 32-bit registry
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(subKey, false);
                return key;
            }
            catch
            {
                return null;
            }
        }
    }
}