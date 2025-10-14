# ThwargZone for Asheron's Call

<div align="center">

![ThwargZone Logo](ThwargZone/Assets/ThwargLogo.ico)

**A nostalgic MSN Gaming Zone-style launcher for Asheron's Call**

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

</div>

---

---

## 📖 Overview

ThwargZone is a feature-rich game launcher for Asheron's Call that brings back the classic MSN Gaming Zone experience. Built on .NET 8.0 with modern Long-Term Support, it provides an easy-to-use interface for connecting to multiple servers, managing game accounts, and launching the game with optional Decal plugin injection.

**Key Highlights:**
- **Modern .NET 8.0 Framework**: Built with the latest .NET technology for reliability and performance
- **Unified Account Management**: Single, elegant interface for managing both username and password
- **Smart Navigation Flow**: Seamless workflow with real-time updates across all windows
- **Classic Windows 95/98 Design**: Authentic recessed borders and form elements for nostalgic appeal
- **Enhanced Custom Server Management**: Comprehensive interface with individual edit/remove buttons
- **Keyboard Navigation**: Full keyboard support with Enter/Escape key handling
- **Background Themes**: Castle-themed backgrounds that change based on server type

---

## ✨ Features

### 🎮 Game Launching
- **One-Click Launch**: Launch Asheron's Call with pre-configured settings
- **Automatic Configuration**: Automatically passes server, account, and password to the game client
- **Multi-Server Support**: Connect to any Asheron's Call server (PvE, PvP, PK, or custom)
- **Working Directory Management**: Properly sets the working directory for the AC client

### 🌐 Server Selection
- **Live Server List**: Automatically fetches and displays available servers from remote sources
- **Real-Time Player Counts**: Shows current player populations for each server
- **Server Sorting**: Servers automatically sorted by player count (custom servers first, then official servers)
- **Server Types**: Visual indicators for PvE, PvP, and PK server types
- **Server Details**: View server descriptions and connection information

### 🛠️ Custom Server Management
- **Comprehensive Management Interface**: Unified window for managing all your custom servers
  - Add new custom servers with name, host, port, type, and description
  - Edit existing custom servers with pre-filled information and individual edit buttons
  - Remove custom servers with individual remove buttons for each server
  - View all custom servers in a scrollable list with full details
- **Smart Navigation Flow**: When editing a custom server, automatically closes GameLauncher and navigates to ServerSelection
- **Real-Time Updates**: Server changes are immediately reflected across all open windows
- **Enhanced User Experience**: Seamless workflow when managing custom servers
- **Persistent Storage**: Custom servers are saved locally in `customServers.xml`

### 🔧 Configuration Options

#### Game Settings
- **Game Location**: Select your Asheron's Call client installation path
- **User Account Management**: Unified interface for managing your game account
  - Single window for both username and password configuration
  - Classic Windows 95/98-style interface with recessed title bar
  - Secure password storage with hidden input fields
  - Keyboard navigation support (Enter to confirm, Escape to cancel)
- **Server Selection**: Choose from available servers with live player counts

#### Advanced Features
- **Decal Injection**: Optional Decal plugin injection at launch
  - Automatically detects Decal installation from registry
  - Injects Decal DLL before game startup
  - Status indicator shows when Decal injection is enabled
- **Remember Server Selection**: Skip server selection on startup
  - Automatically launch with your last selected server
  - Toggle on/off via checkbox in Help menu
  - Saves time for users who play on the same server
- **Background Themes**: Dynamic backgrounds based on server type
  - PvE servers: PvE-themed background
  - PvP/PK servers: PvP-themed background

### 🎨 User Interface
- **Retro MSN Gaming Zone Design**: Authentic throwback to the original MSN Gaming Zone interface
- **Classic Windows 95/98 Design**: Authentic recessed borders and form elements for nostalgic appeal
- **Custom Title Bars**: Windows XP-style title bars with minimize and close buttons
- **Draggable Windows**: Click and drag windows from anywhere on the background
- **Animated Graphics**: Authentic animated GIF support for nostalgic visual elements
- **Window Position Memory**: Automatically remembers window positions between sessions
- **Unified Account Management**: Single, elegant interface for managing both username and password
- **Streamlined Navigation**: Consolidated menu structure with improved user flow

### 📊 Data Sources
- **Server List**: Fetched from `https://raw.githubusercontent.com/acresources/serverslist/master/Servers.xml`
- **Player Counts**: Real-time data from `http://treestats.net/player_counts-latest.json`
- **Custom Servers**: Stored locally in `customServers.xml`

---

## 🚀 Getting Started

### Prerequisites
- Windows operating system
- .NET 8.0 Runtime or later
- Asheron's Call game client installed
- (Optional) Decal plugin installed for injection feature

### Installation
1. Download the latest release from the releases page
2. Extract all files to a folder of your choice
3. Run `ThwargZone.exe`

### First-Time Setup
1. **Launch ThwargZone** - Server Selection window will appear
2. **Choose a Server**:
   - Select a server from the list (this opens the Game Launcher)
3. **Configure Game Location**:
   - Click `Zone` → `Select Game Location`
   - Browse to your `acclient.exe` file (typically in `C:\Turbine\Asheron's Call\`)
4. **Set Up Your Account**:
   - Click `Zone` → `User Account` to open the unified account management window
   - Enter your username and password in the single form
   - Click "Next" or press Enter to save, or "Cancel" or press Escape to cancel
5. **Launch the Game**:
   - Click the "PLAY" button on the main launcher window
6. **(Optional) Remember Server**:
   - Check `Help` → `Remember Server Selection` to skip server selection on next launch

---

## 📋 Usage Guide

### Main Launcher Window

#### Menu Bar

**Room Menu**
- `Server Selection` - Open the server selection window to choose a different server

**Zone Menu**
- `Select Game Location` - Choose your Asheron's Call client executable location
- `User Account` - Open unified account management window for username and password

**Help Menu**
- `Inject Decal` - Toggle Decal plugin injection on/off (checkbox)
- `Remember Server Selection` - Skip server selection screen and launch directly with last selected server (checkbox)
- `Manage Custom Servers` - Open comprehensive custom server management window

#### Main Display
- **Server Name**: Currently selected server displayed at the top
- **Server Description**: Detailed information about the selected server
- **Decal Status**: Shows "with Decal Injection" when enabled
- **Background Image**: Changes based on server type (PvE/PvP)
- **Play Button**: Large, clickable button to launch the game

### Server Selection Window

#### Features
- **Server List**: Scrollable list of all available servers
- **Player Counts**: Real-time player count displayed next to each server name
- **Total Population**: Shows combined player count across all servers
- **Server Types**: Servers labeled with type indicators (PK only, etc.)
- **Quick Select**: Click any server name to instantly open the Game Launcher with that server selected

#### Announcements Section
- Displays classic Asheron's Call-themed announcements
- Retro graphics and styling

#### Navigation Bar
- `Game Help` - Information about game help
- `Member Services` - Opens the unified account management window
- `Subscription` - Opens the custom server management window
- `Website` - Opens https://asheronscall.com
- `Exit` - Closes the application

### Account Management Window

A unified interface for managing your game account credentials:
- **Classic Design**: Windows 95/98-style recessed title bar and form elements
- **Single Form**: Enter both username and password in one window
- **Keyboard Navigation**: 
  - Press **Enter** to save and close
  - Press **Escape** to cancel and close
- **Background**: Castle-themed background image for nostalgic appeal
- **Navigation Buttons**: Support, Quit, and Next buttons at the bottom

### Manage Custom Servers Window

Comprehensive interface for managing all your custom servers:
- **Server List**: View all custom servers with full details in a scrollable list
- **Individual Actions**: Each server has its own Edit and Remove buttons
- **Edit Functionality**: Click "Edit" to modify an existing server with pre-filled information
- **Remove Functionality**: Click "Remove" to delete a specific server
- **Add New Server**: Button to add additional custom servers
- **No Servers Message**: Helpful message when no custom servers are configured
- **Smart Navigation**: After editing a server, automatically closes GameLauncher and returns to ServerSelection
- **Real-Time Updates**: All changes are immediately reflected across open windows

### Add/Edit Custom Server Window

Enter or modify the following information:
- **Server Name**: Display name for your server
- **Server Host**: IP address or hostname
- **Server Port**: Port number (typically 9000-9004)
- **Server Type**: Select from dropdown (PvE/PK/PvP/Test)
- **Description**: Optional description of the server

- **Add Mode**: Click `Add Server` to save a new server, or `Cancel` to discard
- **Edit Mode**: Click `Save` to update an existing server, or `Cancel` to discard changes

---

## ⚙️ Configuration Files

### Settings Location
Settings are stored in the application's user configuration:
- `%LOCALAPPDATA%\ThwargZone\`

### Saved Settings
- `ACLocation` - Path to acclient.exe
- `ServerPath` - Current server host:port
- `ServerName` - Current server display name
- `ServerDescription` - Current server description
- `ServerType` - Current server type (PvE/PvP/PK)
- `UserAccount` - Game account username
- `UserPassword` - Game account password (encrypted by .NET)
- `InjectDecal` - Decal injection toggle state
- `RememberServerSelection` - Skip server selection screen on startup
- Window positions and sizes

### Custom Servers File
`customServers.xml` in the application directory:
```xml
<Servers>
  <ServerItem>
    <name>My Server</name>
    <server_host>127.0.0.1</server_host>
    <server_port>9000</server_port>
    <type>PvE</type>
    <description>My custom server description</description>
  </ServerItem>
</Servers>
```

---

## 🔌 Decal Integration

### How It Works
1. ThwargZone detects your Decal installation via Windows Registry
2. Launches the AC client in suspended mode
3. Injects the Decal DLL into the process
4. Resumes the game with Decal loaded

### Requirements
- Decal must be properly installed and registered in the Windows Registry
- Compatible Decal version for your AC client
- Registry key: `HKEY_CURRENT_USER\Software\Decal\Decal`

### Troubleshooting
If Decal injection fails:
1. Verify Decal is properly installed
2. Run ThwargZone as Administrator
3. Check that your antivirus isn't blocking the injection
4. Ensure Decal is compatible with your AC client version

---

## 🛡️ Security Notes

- Passwords are stored using .NET's built-in settings encryption
- No data is transmitted to third parties (except fetching public server lists)
- All game credentials are passed directly to the AC client
- Decal injection uses standard Windows process injection techniques

---

## 🐛 Troubleshooting

### Game Won't Launch
- Verify your AC client path is correct
- Ensure `acclient.exe` exists at the configured location
- Check that you have a server, account, and password configured

### Decal Injection Fails
- Try running ThwargZone as Administrator
- Verify Decal is installed correctly
- Check Windows Registry for Decal installation
- Disable antivirus temporarily to test

### Server List Not Loading
- Check your internet connection
- Verify firewall isn't blocking HTTP requests
- Server list URL may be temporarily unavailable

### Window Positions Not Saving
- Check that the application has write permissions to `%LOCALAPPDATA%`
- Try running as Administrator

---

## 🏗️ Technical Details

### Built With
- **.NET 8.0** - Modern .NET framework with Long-Term Support (LTS) for reliability and performance
- **WPF (Windows Presentation Foundation)** - UI framework
- **AnimatedImage.Wpf** - Animated GIF support
- **Newtonsoft.Json** - JSON parsing for player counts
- **Custom Win32 Injection** - Decal DLL injection
- **System.Drawing.Common 8.0.0** - Enhanced graphics support
- **Production-Ready Codebase** - Clean, optimized code with all diagnostic/debug logging removed

### Architecture
- **MVVM Pattern** - Clean separation of UI and logic
- **RelayCommand** - Command pattern for UI actions
- **Settings Persistence** - Built-in .NET settings system
- **Window Placement** - Custom window position memory system

### Dependencies
- `AnimatedImage.dll` - Animated GIF rendering
- `AnimatedImage.Wpf.dll` - WPF integration for animations
- `Newtonsoft.Json.dll` - JSON serialization
- `Decal.Adapter.dll` - Decal integration
- `injector.dll` - Process injection library
- `VCS5.dll` - Additional game support

---

## 📝 License

This project is open source and available under the MIT License.

---

## 🙏 Acknowledgments

- **Asheron's Call Community** - For keeping the game alive
- **Turbine Entertainment** - Original creators of Asheron's Call
- **Decal Plugin Team** - For the amazing plugin framework
- **AC Resources Team** - For maintaining the public server list
- **TreeStats** - For providing real-time player count data

---

## 📧 Support

For issues, questions, or contributions, please visit the project repository or contact the development team.

---

<div align="center">

**Made with ❤️ for the Asheron's Call Community**

*"May the shadows never find you."*

</div>

