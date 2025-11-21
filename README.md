# Aliyun OSS Manager

A modern Windows desktop application for managing Aliyun Object Storage Service (OSS) buckets with an intuitive WPF interface.

## Features

### Core Functionality
- **Folder Management**
  - Create new folders
  - Navigate through folder hierarchy
  - View folder contents with breadcrumb navigation
  - Path history for quick access

- **File Upload**
  - Upload files to any folder
  - Drag and drop support (via folder selection)
  - Preserve folder structure

- **File Download**
  - Download individual files
  - Save files to any local location
  - Maintain original file names

- **Delete Operations**
  - Delete single or multiple files
  - Delete folders (including all contents)
  - Multi-select support with checkboxes and row selection
  - Confirmation prompts for safety

- **URL Generation**
  - Generate presigned URLs for files
  - 1-hour validity period
  - Automatic clipboard copy

- **Batch Move**
  - Move multiple files between folders
  - Smart folder detection (scans entire bucket)
  - Skip files already in target location
  - Visual folder selection dialog

### User Interface
- **Modern Design**
  - Clean and intuitive interface
  - Icon-based file/folder indicators (folder/file icons)
  - Color-coded elements for better visibility
  - Alternating row colors in data grid

- **Smart Sorting**
  - Sort by last modified time (ascending/descending)
  - Sort by file name (A-Z / Z-A)
  - Sort by file size (ascending/descending)
  - Folders always appear first

- **Status Information**
  - Real-time status updates
  - File count display
  - Selection counter (checkboxes + row selection)
  - Operation progress indicators

### Configuration
- **Configuration Management**
  - Save OSS credentials locally
  - Config file stored in application directory (portable)
  - Auto-fill on startup
  - Clear saved configuration option

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- Aliyun OSS account with valid credentials

## Installation

1. Download the latest release
2. Extract to any folder
3. Run `WpfApp1.exe`

**Note**: Configuration file (`config.json`) is saved in the same folder as the executable, making the application fully portable.

## Usage

### Initial Setup

1. **Connect to OSS**
   - Enter your AccessKeyId
   - Enter your AccessKeySecret
   - Enter the Endpoint (e.g., `oss-cn-hangzhou.aliyuncs.com`)
   - Enter your Bucket Name
   - Check "Remember Config" to save credentials
   - Click "Connect"

### File Operations

#### Upload Files
1. Navigate to the target folder (or stay in root)
2. Click "Upload File"
3. Select file from your computer
4. File will be uploaded to current path

#### Download Files
1. Select a file in the list
2. Click "Download File"
3. Choose save location
4. File will be downloaded

#### Delete Files/Folders
1. Select items using checkboxes or row selection (Ctrl/Shift supported)
2. Click "Delete"
3. Confirm the operation
4. Selected items will be deleted

#### Batch Move Files
1. Select multiple files using checkboxes or row selection
2. Click "Batch Move"
3. Select target folder from the dialog
4. Confirm the move operation
5. Files will be moved to the target folder

### Navigation

- **Up Button**: Go to parent directory
- **Root Button**: Return to root directory
- **Path ComboBox**: 
  - Type a path and press Enter to navigate
  - Select from dropdown for quick navigation
  - Shows current path and history

### Folder Management

#### Create New Folder
1. Navigate to where you want to create the folder
2. Click "New Folder"
3. Enter folder name
4. Folder will be created at current path

**Note**: 
- Folder names cannot contain `/` or `\` characters
- Use simple names like "backup" or paths like "files/2024" for nested folders

## Configuration File

The application stores configuration in `config.json` in the same directory as the executable:

```json
{
  "AccessKeyId": "your-access-key-id",
  "AccessKeySecret": "your-access-key-secret",
  "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
  "BucketName": "your-bucket-name"
}
```

**Security Note**: Keep this file secure as it contains your OSS credentials.

## Project Structure

```
WpfApp1/
??? MainWindow.xaml(.cs)       # Main application window
??? FolderSelectDialog.xaml(.cs)  # Folder selection dialog
??? InputDialog.xaml(.cs)      # Input dialog for new folders
??? Models/
?   ??? OssConfig.cs        # OSS configuration model
?   ??? OssFileInfo.cs       # File information model
??? Services/
?   ??? OssService.cs         # OSS operations service
?   ??? ConfigService.cs          # Configuration management
??? config.json      # Saved configuration (created on first save)
```

## Technical Details

### Built With
- **.NET 8.0** - Target framework
- **WPF** - User interface framework
- **Aliyun.OSS.SDK** - Aliyun OSS SDK for .NET
- **C# 12.0** - Programming language

### Key Features Implementation

#### Folder Detection
The application uses a smart folder detection algorithm:
1. Scans all objects in the bucket
2. Extracts folder paths from file keys
3. Detects folder marker objects (keys ending with `/`)
4. Builds complete folder hierarchy

#### Multi-Selection
Supports two selection methods:
- **Checkbox selection**: Persistent across navigation
- **Row selection**: Standard grid selection with Ctrl/Shift

Total selection = Unique items from both methods

#### File Sorting
- Folders always appear first
- Files sorted according to selected criteria
- Six sorting options available

## Troubleshooting

### Connection Failed
- Verify your AccessKeyId and AccessKeySecret are correct
- Check that the Endpoint matches your bucket's region
- Ensure the Bucket Name is spelled correctly
- Check your network connection

### Folders Not Showing in Batch Move
- Click "Refresh List" to reload the file list
- The application scans all files to detect folders
- Empty folders (with no files) may not appear

### Configuration Not Saving
- Ensure the application has write permissions in its directory
- Check that the folder is not read-only
- Try running as administrator if necessary

## Keyboard Shortcuts

- **Enter** (in path box): Navigate to entered path
- **Ctrl + Click**: Multi-select files (row selection)
- **Shift + Click**: Range select files (row selection)
- **Double Click on Folder**: Navigate into folder
- **Double Click on File**: Prompt to download

## License

This project is provided as-is for personal or commercial use.

## Credits

Developed using:
- Aliyun OSS SDK for .NET
- WPF (Windows Presentation Foundation)
- .NET 8.0

## Support

For issues, questions, or suggestions:
1. Check the Troubleshooting section
2. Review Aliyun OSS documentation
3. Verify your OSS account permissions

## Version History

### Latest Version
- [x] English localization complete
- [x] Batch move files functionality
- [x] Smart folder detection algorithm
- [x] Portable configuration (saved in app directory)
- [x] Resizable folder selection dialog
- [x] Multi-selection support (checkbox + row)
- [x] Comprehensive file operations
- [x] Modern UI with icons

## Screenshots

### Main Window
The main window displays files and folders with intuitive icons and color coding:
- Folders appear first, highlighted in gold
- Files are listed below with size and modification time
- Multi-selection via checkboxes and row selection

### Batch Move Dialog
The folder selection dialog allows you to:
- View all available folders in your bucket
- See your current location
- Select target folder with visual feedback

---

**Note**: This application requires valid Aliyun OSS credentials. Never share your `config.json` file as it contains sensitive access keys.
