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

- Windows 10 or higher
- .NET 7.0 Runtime or higher

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
cd WinLockTimer
dotnet build
dotnet run
```

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