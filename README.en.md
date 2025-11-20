# WinLockTimer - Parental Control Computer Time Management

<div align="right">
  <b>Language:</b>
  <a href="README.md">中文</a> |
  <a href="README.en.md">English</a>
</div>

## Project Overview

WinLockTimer is a simple Windows desktop application designed specifically for parents to control their children's computer usage time. The program starts a countdown timer, displays reminders before the countdown ends, and automatically locks the screen when the countdown finishes. It includes password protection to prevent children from stopping the timer without permission.

## Features

### Core Functions
- **Countdown Timer**: Support for hours and minutes settings
- **Reminder Notifications**: Pop-up reminders at 10, 5, 4, 3, 2, and 1 minutes before end
- **Auto Lock Screen**: Automatically executes Windows lock screen when countdown ends
- **Pause/Resume**: Support for pausing and resuming countdown
- **Reset Function**: Reset countdown at any time
- **Single Instance**: Prevents multiple instances from running simultaneously

### Security Features
- **Parental Password Protection**: After setting a parent password, pause, reset, and closing the program require password verification
- **AES Encryption Storage**: Parent password is stored locally using AES-128 encryption
- **Multiple Reminder Methods**: Support for popup reminders, voice reminders, or both
- **Settings Persistence**: Password and reminder settings are automatically saved and restored

### User Interface
- Clean and intuitive Chinese interface
- Large font display for remaining time
- Real-time status display
- Color-coded alerts (Blue → Orange → Red)

## System Requirements

- **Operating System**: Windows 10 or higher
- **Runtime**: .NET 7.0 Runtime or higher
- **Architecture**: x64 (64-bit)
- **Permissions**: No administrator privileges required

## Download and Installation

### Release Package Structure
Standard release package contains the following files:
```
WinLockTimer/
├── WinLockTimer.exe              # Main program file (required)
├── WinLockTimer.dll              # Program assembly (required)
├── BCrypt.Net-Next.dll           # Encryption library (required)
├── WinLockTimer.runtimeconfig.json  # Runtime configuration (required)
├── WinLockTimer.deps.json        # Dependency configuration (required)
├── 使用说明.md                    # user manual for Chinese (recommand)
├── User_Manual.md                # user manual for English (recommand)
└── WinLockTimer.settings         # User settings (auto-generated)
```

### User Installation Steps
1. **Download**: Download the latest WinLockTimer release package from GitHub Releases page
2. **Extract**: Extract the zip file to any folder (e.g., C:\\WinLockTimer)
3. **Install Runtime**: If .NET 7.0 Runtime is not installed, please [download and install](https://dotnet.microsoft.com/download/dotnet/7.0) it first
4. **Run**: Double-click `WinLockTimer.exe` to start the program
5. **First Use**: The program will automatically create `WinLockTimer.settings` file to save user settings

### Important Notes
- **Security Software**: Some antivirus software may give false positives, please add the program to your trust list
- **First Run**: The program will automatically create a user settings file, no manual creation needed
- **Uninstall**: Simply delete the program folder, no system residue will be left

## Usage

### Basic Usage
1. **Set Time**: Enter hours and minutes in the "Set Time" area
2. **Start Countdown**: Click the "Start" button to begin countdown
3. **Pause/Resume**: Click "Pause" button to pause, click "Resume" button to continue
4. **Reset**: Click "Reset" button to stop and reset countdown
5. **Auto Reminders**: Program will pop up reminders at specified time points
6. **Auto Lock Screen**: Automatically locks screen when countdown ends

### Parental Password Protection
1. **Set Password**: Enter password in the "Parent Password" field in settings area
2. **Choose Reminder Method**: Select reminder method from dropdown (Popup, Voice, or Both)
3. **Password Verification**: After setting password, pause, reset, and closing the program require password input
4. **Clear Settings**: Click "Clear Settings" button to delete saved password and settings

### Reminder Methods
- **Popup Reminder**: Display text reminder dialog
- **Voice Reminder**: Play system notification sound
- **Both Combined**: Show popup and play sound simultaneously

## Reminder Time Points

- 10 minutes before countdown ends
- 5 minutes before countdown ends
- 4 minutes before countdown ends
- 3 minutes before countdown ends
- 2 minutes before countdown ends
- 1 minute before countdown ends (special reminder)

## Technical Implementation

- **Development Language**: C#
- **GUI Framework**: Windows Forms (WinForms)
- **Target Platform**: Windows 10/11
- **.NET Version**: .NET 7.0
- **Encryption Algorithm**: AES-128 (CBC mode, PKCS7 padding)
- **Data Storage**: JSON format settings file, password encrypted with AES
- **Lock Screen Mechanism**: Windows system command lock screen

## Build Instructions

### Using Visual Studio
1. Open `WinLockTimer.sln`
2. Build the project
3. Run the program

### Using .NET CLI
```bash
# Clone repository
git clone https://github.com/caodianlong/LockWindowsTimer.git
cd LockWindowsTimer

# Build Debug version
dotnet build -c Debug

# Build Release version
dotnet build -c Release

# Create release version (recommended)
dotnet publish -c Release --self-contained false
```

### Release Build
After building, release files are located at: `WinLockTimer/bin/Release/net7.0-windows/publish/`

## Deployment and Release

### Creating Release Package
```bash
# Navigate to publish directory
cd WinLockTimer/bin/Release/net7.0-windows/publish/

# Create compressed package (Windows)
tar -a -c -f WinLockTimer-v1.2.0.zip *.exe *.dll *.json *.md

# Or manually select files to compress
# Required files: WinLockTimer.exe, WinLockTimer.dll, BCrypt.Net-Next.dll, *.json
# Recommended files: README.md, README.en.md
```

### GitHub Releases
1. Create new Release tag (e.g., v1.2.0)
2. Upload the built release package
3. Add version description and changelog
4. Publish the Release

## Deployment

The program can be built as a standalone executable file that runs without installation.

## Security Notes

### Password Storage Security
- Parent password is encrypted and stored using AES-128 algorithm
- Encryption key is fixed and stored within the program
- Password is saved in Base64 encoded encrypted form in local settings file
- Password is automatically decrypted and loaded when program starts

### Operation Protection
- After setting password, sensitive operations (pause, reset, close) require password verification
- Closing the program during countdown requires password confirmation
- Clear Settings function can safely delete saved passwords

## Important Notes

- Program uses standard Windows lock screen function, does not modify system settings
- Program checks for running countdown when exiting
- Program interface is simple and suitable for parents
- Password protection is optional, direct operation available without setting password
- Reminder methods can be selected as needed to avoid disturbance

## Update Log

### v1.2.0 (Latest)
- Added single instance prevention feature
- Implemented window activation on duplicate launch
- Enhanced user experience by removing popup notifications

### v1.1.0
- Added parental password protection feature
- Implemented AES-128 encrypted password storage
- Added multiple reminder method selection
- Added settings persistence functionality
- Optimized user interface and operation flow

### v1.0.0
- Basic countdown functionality
- Multi-time point reminders
- Auto lock screen feature
- Pause/Resume/Reset controls

## License

This project is open source software following the MIT License.
