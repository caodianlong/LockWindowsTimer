# WinLockTimer User Manual

## Software Introduction

WinLockTimer is a computer time management software specifically designed for parents. By setting countdown timers, it helps parents manage their children's computer usage time effectively. When the countdown ends, the program automatically locks the screen to protect children's eye health.

## System Requirements

- **Operating System**: Windows 10 or higher
- **Runtime Environment**: .NET 7.0 Runtime required
- **Hardware**: Regular computer can run the program

## Quick Start

### Step 1: Download and Install
1. Download the latest WinLockTimer package from the official website or GitHub
2. Extract to any folder (e.g., Desktop or D: drive)
3. If prompted for .NET Runtime, download and install it first

### Step 2: Launch the Program
Double-click the `WinLockTimer.exe` file to start the program

### Step 3: Basic Usage
1. **Set Time**: Enter hours and minutes in the "Set Time" area
2. **Start Timer**: Click the "Start" button to begin countdown
3. **Check Status**: The program displays remaining time and current status

## Detailed Function Description

### 1. Countdown Settings
- **Hour Setting**: Enter numbers between 0-23
- **Minute Setting**: Enter numbers between 0-59
- **Example**: To set 1 hour 30 minutes, enter "1" hour and "30" minutes

### 2. Control Buttons
- **Start**: Begin countdown
- **Pause**: Pause current countdown
- **Resume**: Resume countdown from paused state
- **Reset**: Stop and reset countdown

### 3. Display Area
- **Remaining Time**: Large font display of remaining hours and minutes
- **Status Indicator**: Shows "Running", "Paused", "Stopped" status
- **Color Changes**:
  - Blue: Normal countdown
  - Orange: Approaching end (within 10 minutes)
  - Red: About to end (within 5 minutes)

## Parental Password Protection

### Setting Parent Password
1. Enter password in the "Parent Password" field in Settings area
2. Password can be any combination of characters
3. Password is automatically saved after setting

### Password Protected Operations
After setting password, the following operations require password input:
- Pause countdown
- Reset countdown
- Close program (when countdown is running)

### Clear Password Settings
Click "Clear Settings" button to delete saved password and settings

## Reminder Features

### Reminder Time Points
The program automatically reminds at these time points:
- 10 minutes before end
- 5 minutes before end
- 4 minutes before end
- 3 minutes before end
- 2 minutes before end
- 1 minute before end (special reminder)

### Reminder Method Selection
In the "Settings" area, choose reminder method:
- **Popup Reminder**: Display text reminder dialog
- **Voice Reminder**: Play system notification sound
- **Both Combined**: Show popup and play sound simultaneously

## Auto Lock Screen Feature

- When countdown ends, the program automatically executes Windows lock screen
- After locking, Windows login password is required to continue using computer
- This is standard Windows security feature, does not modify system settings

## Frequently Asked Questions

### Q: What if the program won't start?
A: Please check if .NET 7.0 Runtime is installed. You can download it from Microsoft's official website.

### Q: How to completely uninstall the program?
A: Simply delete the entire program folder.

## Usage Recommendations

### Before Giving to Children
1. Test and set parent password first
2. Choose appropriate reminder method
3. Explain usage rules to children

---

**Friendly Reminder**: This program aims to help parents manage children's computer usage time effectively. We recommend communicating fully with children to establish good usage habits.
