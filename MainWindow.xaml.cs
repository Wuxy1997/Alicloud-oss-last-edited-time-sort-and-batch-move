using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Models;
using WpfApp1.Services;
using IOPath = System.IO.Path;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
 private readonly OssService _ossService;
        private List<OssFileInfo> _allFiles;
     private string _currentPath = "";
        private List<string> _pathHistory;

        public MainWindow()
   {
    InitializeComponent();
 _ossService = new OssService();
            _allFiles = new List<OssFileInfo>();
         _pathHistory = new List<string>();

    dgFiles.LoadingRow += (s, e) =>
        {
       if (e.Row.Item is OssFileInfo fileInfo)
      {
            fileInfo.PropertyChanged += (sender, args) =>
                {
       if (args.PropertyName == nameof(OssFileInfo.IsSelected))
        {
    UpdateSelectedCount();
       }
        };
         }
   };

      dgFiles.SelectionChanged += (s, e) => UpdateSelectedCount();

     cmbCurrentPath.KeyDown += (s, e) =>
   {
         if (e.Key == Key.Enter)
        {
      NavigateToPath(cmbCurrentPath.Text);
             }
    };

      LoadSavedConfig();
    }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAccessKeyId.Text) ||
        string.IsNullOrWhiteSpace(txtAccessKeySecret.Password) ||
      string.IsNullOrWhiteSpace(txtEndpoint.Text) ||
    string.IsNullOrWhiteSpace(txtBucketName.Text))
            {
         MessageBox.Show("Please fill in complete OSS configuration information!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
   }

        txtStatus.Text = "Connecting...";
            btnConnect.IsEnabled = false;

        var config = new OssConfig
            {
                AccessKeyId = txtAccessKeyId.Text.Trim(),
     AccessKeySecret = txtAccessKeySecret.Password.Trim(),
      Endpoint = txtEndpoint.Text.Trim(),
                BucketName = txtBucketName.Text.Trim()
          };

       if (chkRememberConfig.IsChecked == true)
 {
      ConfigService.SaveConfig(config);
}

            Task.Run(() =>
      {
         bool success = _ossService.Initialize(config);

          Dispatcher.Invoke(() =>
           {
             btnConnect.IsEnabled = true;

            if (success)
      {
  txtStatus.Text = "Connected successfully!";
            EnableOperationButtons(true);
       // Initialize to root directory
        _currentPath = "";
            // Initialize path navigation
     UpdatePathComboBox();
              // Load root directory files
               LoadFiles();
          }
   else
     {
     txtStatus.Text = "Connection failed, please check configuration!";
    MessageBox.Show("Connection failed, please check if the configuration information is correct!", "Error",
       MessageBoxButton.OK, MessageBoxImage.Error);
    }
                });
        });
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
   LoadFiles();
        }

        private void DgFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFiles.SelectedItem is OssFileInfo selectedItem)
            {
   if (selectedItem.IsFolder)
                {
        NavigateToPath(selectedItem.FullPath);
     }
            else
    {
       var result = MessageBox.Show($"Do you want to download file {selectedItem.FileName}?", "Tip",
           MessageBoxButton.YesNo, MessageBoxImage.Question);
     if (result == MessageBoxResult.Yes)
   {
             BtnDownload_Click(sender, new RoutedEventArgs());
   }
        }
            }
        }

        private void NavigateToPath(string path)
   {
            if (path == null) path = "";
            path = path.Trim();

        // Handle special case of root directory
       if (path == "/")
      {
    path = "";
            }

            // Ensure non-root directory paths end with /
   if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
  {
                path += "/";
          }

            _currentPath = path;
        LoadFiles();
     UpdatePathComboBox();
        }

        private void BtnGoUp_Click(object sender, RoutedEventArgs e)
  {
            if (string.IsNullOrEmpty(_currentPath))
  return;

      string path = _currentPath.TrimEnd('/');
   int lastSlash = path.LastIndexOf('/');
   if (lastSlash >= 0)
            {
        path = path.Substring(0, lastSlash + 1);
        }
     else
            {
        path = "";
   }

            NavigateToPath(path);
        }

      private void BtnGoRoot_Click(object sender, RoutedEventArgs e)
    {
      NavigateToPath("");
        }

        private void CmbCurrentPath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
       // Avoid triggering on programmatic changes
if (e.AddedItems.Count == 0)
    return;

      if (cmbCurrentPath.SelectedItem is string selectedPath)
        {
        // Only navigate when selected path differs from current path
    string normalizedPath = selectedPath == "/" ? "" : selectedPath;
      if (normalizedPath != _currentPath)
    {
      NavigateToPath(selectedPath);
      }
            }
        }

        private void UpdatePathComboBox()
        {
            cmbCurrentPath.Items.Clear();

 // Always add root directory option
            cmbCurrentPath.Items.Add("/");

  if (!string.IsNullOrEmpty(_currentPath))
         {
  // Add all levels of current path
    string[] parts = _currentPath.TrimEnd('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
         string accumulatedPath = "";
    foreach (var part in parts)
            {
        accumulatedPath += part + "/";
     if (!cmbCurrentPath.Items.Contains(accumulatedPath))
             {
        cmbCurrentPath.Items.Add(accumulatedPath);
     }
       }
  }

            // Set current display text
       cmbCurrentPath.Text = string.IsNullOrEmpty(_currentPath) ? "/" : _currentPath;

       // Add history (no duplicates)
  foreach (var historyPath in _pathHistory.Distinct().Reverse().Take(10))
  {
       if (!cmbCurrentPath.Items.Contains(historyPath))
           {
     cmbCurrentPath.Items.Add(historyPath);
       }
       }

     // Update button states
  btnGoUp.IsEnabled = !string.IsNullOrEmpty(_currentPath);
       btnGoRoot.IsEnabled = !string.IsNullOrEmpty(_currentPath);
        }

        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
      var inputDialog = new InputDialog("New Folder", "Please enter folder name:", "");
     if (inputDialog.ShowDialog() == true)
     {
           string folderName = inputDialog.Answer.Trim();

      if (string.IsNullOrEmpty(folderName))
      {
   MessageBox.Show("Folder name cannot be empty!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
     }

  if (folderName.Contains("/") || folderName.Contains("\\"))
             {
   MessageBox.Show("Folder name cannot contain / or \\ characters!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
         return;
       }

      string fullPath = _currentPath + folderName + "/";

    txtStatus.Text = $"Creating folder {folderName}...";
    EnableOperationButtons(false);

                try
{
            bool success = _ossService.CreateFolder(fullPath);

                 if (success)
    {
     txtStatus.Text = $"Folder {folderName} created successfully!";
       MessageBox.Show("Folder created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
             LoadFiles();
           }
    else
   {
              txtStatus.Text = "Creation failed!";
      MessageBox.Show("Folder creation failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
     }
         catch (Exception ex)
       {
     txtStatus.Text = "Creation failed!";
          MessageBox.Show($"Creation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
                finally
      {
           EnableOperationButtons(true);
            }
   }
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
 {
          Title = "Select file to upload",
       Filter = "All files|*.*",
        Multiselect = false
   };

       if (openFileDialog.ShowDialog() == true)
            {
            string localFilePath = openFileDialog.FileName;
          string fileName = IOPath.GetFileName(localFilePath);
    string ossKey = _currentPath + fileName;

        txtStatus.Text = $"Uploading {fileName}...";
          EnableOperationButtons(false);

          try
       {
        bool success = await _ossService.UploadFileAsync(localFilePath, ossKey);

      if (success)
          {
      txtStatus.Text = $"File {fileName} uploaded successfully!";
         MessageBox.Show($"File uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    LoadFiles();
          }
               else
     {
         txtStatus.Text = "Upload failed!";
     MessageBox.Show("File upload failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
       catch (Exception ex)
      {
    txtStatus.Text = "Upload failed!";
           MessageBox.Show($"Upload failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
          finally
{
           EnableOperationButtons(true);
                }
         }
   }

private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = dgFiles.SelectedItem as OssFileInfo;
       if (selectedFile == null)
            {
       MessageBox.Show("Please select a file to download first!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
return;
   }

          if (selectedFile.IsFolder)
            {
        MessageBox.Show("Cannot download folders!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
  return;
            }

        var saveFileDialog = new SaveFileDialog
     {
              Title = "Save file",
             FileName = selectedFile.FileName,
       Filter = "All files|*.*"
            };

         if (saveFileDialog.ShowDialog() == true)
       {
          txtStatus.Text = $"Downloading {selectedFile.FileName}...";
    EnableOperationButtons(false);

   try
           {
     bool success = await _ossService.DownloadFileAsync(selectedFile.FullPath, saveFileDialog.FileName);

   if (success)
    {
             txtStatus.Text = $"File {selectedFile.FileName} downloaded successfully!";
        MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
   }
             else
   {
     txtStatus.Text = "Download failed!";
     MessageBox.Show("File download failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
       }
         catch (Exception ex)
      {
           txtStatus.Text = "Download failed!";
     MessageBox.Show($"Download failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
     finally
                {
                    EnableOperationButtons(true);
       }
 }
        }

      private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
    var checkedFiles = _allFiles.Where(f => f.IsSelected).ToList();
            var selectedRows = dgFiles.SelectedItems.Cast<OssFileInfo>().ToList();
            var selectedFiles = checkedFiles.Union(selectedRows).Distinct().ToList();

            if (selectedFiles.Count == 0)
            {
   MessageBox.Show("Please select items to delete first!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
            }

   var folders = selectedFiles.Where(f => f.IsFolder).ToList();
            var files = selectedFiles.Where(f => !f.IsFolder).ToList();

string message = "";
      if (folders.Count > 0 && files.Count > 0)
            {
              message = $"Are you sure you want to delete {folders.Count} folder(s) and {files.Count} file(s)?\n\nWarning: Deleting folders will also delete all files inside!\nThis operation cannot be undone!";
  }
            else if (folders.Count > 0)
         {
         message = $"Are you sure you want to delete {folders.Count} folder(s)?\n\nWarning: Deleting folders will also delete all files inside!\nThis operation cannot be undone!";
   }
    else
            {
            message = $"Are you sure you want to delete {files.Count} file(s)?\n\nThis operation cannot be undone!";
            }

            var result = MessageBox.Show(message, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes)
          {
        txtStatus.Text = $"Deleting {selectedFiles.Count} item(s)...";
       EnableOperationButtons(false);

                Task.Run(async () =>
   {
           try
       {
   int successCount = 0;
             int failedCount = 0;

        foreach (var file in files)
{
       try
{
    _ossService.DeleteFile(file.FullPath);
     successCount++;
            }
   catch
    {
         failedCount++;
    }
     }

             foreach (var folder in folders)
  {
           try
        {
        await _ossService.DeleteFolderAsync(folder.FullPath);
      successCount++;
          }
         catch
       {
            failedCount++;
       }
     }

  Dispatcher.Invoke(() =>
     {
   txtStatus.Text = $"Deletion complete: {successCount} succeeded, {failedCount} failed";
                    MessageBox.Show($"Deletion complete!\nSucceeded: {successCount}\nFailed: {failedCount}",
   "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
       LoadFiles();
   });
        }
        catch (Exception ex)
          {
          Dispatcher.Invoke(() =>
       {
   txtStatus.Text = "Deletion failed!";
            MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
 });
         }
          finally
        {
         Dispatcher.Invoke(() => EnableOperationButtons(true));
     }
     });
      }
        }

      private void BtnGetUrl_Click(object sender, RoutedEventArgs e)
        {
     var selectedFile = dgFiles.SelectedItem as OssFileInfo;
            if (selectedFile == null)
    {
   MessageBox.Show("Please select a file first!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
     return;
            }

 if (selectedFile.IsFolder)
            {
    MessageBox.Show("Cannot get URL for folders!", "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
    return;
 }

            try
        {
   string url = _ossService.GetFileUrl(selectedFile.FullPath);
        if (!string.IsNullOrEmpty(url))
 {
   Clipboard.SetText(url);
       MessageBox.Show($"File URL copied to clipboard!\n\n{url}\n\n(URL valid for 1 hour)",
           "Success", MessageBoxButton.OK, MessageBoxImage.Information);
      txtStatus.Text = "URL copied to clipboard";
 }
 else
   {
        MessageBox.Show("Failed to get URL!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
     }
    }
        catch (Exception ex)
   {
    MessageBox.Show($"Failed to get URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbSortOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allFiles == null || _allFiles.Count == 0)
       return;

            ApplySorting();
        }

        private void LoadFiles()
   {
            txtStatus.Text = "Loading file list...";
    EnableOperationButtons(false);

            Task.Run(() =>
      {
                try
   {
var files = _ossService.ListFiles(_currentPath);

       Dispatcher.Invoke(() =>
      {
        _allFiles = files;
      _allFiles = _allFiles.OrderByDescending(f => f.IsFolder)
 .ThenBy(f => f.FileName)
    .ToList();

            ApplySorting();
 txtFileCount.Text = $"Files: {_allFiles.Count(f => !f.IsFolder)} Folders: {_allFiles.Count(f => f.IsFolder)}";
        txtStatus.Text = "File list loaded";

        UpdatePathComboBox();

    if (!_pathHistory.Contains(_currentPath))
               {
       _pathHistory.Add(_currentPath);
    }
});
     }
            catch (Exception ex)
    {
           Dispatcher.Invoke(() =>
                {
   txtStatus.Text = "Loading failed!";
            MessageBox.Show($"Failed to load file list: {ex.Message}", "Error",
      MessageBoxButton.OK, MessageBoxImage.Error);
        });
                }
        finally
        {
          Dispatcher.Invoke(() => EnableOperationButtons(true));
          }
  });
      }

        private void ApplySorting()
        {
  if (_allFiles == null || _allFiles.Count == 0)
      {
     dgFiles.ItemsSource = null;
return;
    }

            var folders = _allFiles.Where(f => f.IsFolder).ToList();
            var files = _allFiles.Where(f => !f.IsFolder).ToList();

     IEnumerable<OssFileInfo> sortedFilesList = files;

      switch (cmbSortOrder.SelectedIndex)
  {
                case 0:
         sortedFilesList = files.OrderByDescending(f => f.LastModified);
     break;
 case 1:
    sortedFilesList = files.OrderBy(f => f.LastModified);
          break;
    case 2:
        sortedFilesList = files.OrderBy(f => f.FileName);
 break;
      case 3:
    sortedFilesList = files.OrderByDescending(f => f.FileName);
         break;
      case 4:
    sortedFilesList = files.OrderByDescending(f => f.Size);
              break;
   case 5:
   sortedFilesList = files.OrderBy(f => f.Size);
           break;
            }

            var sortedFiles = folders.Concat(sortedFilesList);

   dgFiles.ItemsSource = sortedFiles.ToList();
       UpdateSelectedCount();
        }

     private async void BtnBatchMove_Click(object sender, RoutedEventArgs e)
        {
  var checkedFiles = _allFiles.Where(f => f.IsSelected && !f.IsFolder).ToList();
  var selectedRows = dgFiles.SelectedItems.Cast<OssFileInfo>().Where(f => !f.IsFolder).ToList();
   var selectedFiles = checkedFiles.Union(selectedRows).Distinct().ToList();

    if (selectedFiles.Count == 0)
         {
   MessageBox.Show("Please select files to move first!\n\nTips:\n1. You can check checkboxes\n2. You can click rows to select directly (Ctrl/Shift multi-select supported)\n3. Only files can be moved, not folders",
           "Tip", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
    }

      try
   {
          // Get all folder list
  txtStatus.Text = "Loading folder list...";
  
         List<string> allFolders;
         try
    {
  allFolders = _ossService.GetAllFolders();
       txtStatus.Text = $"Found {allFolders.Count} folder(s)";
         }
         catch (Exception ex)
         {
     txtStatus.Text = "Failed to load folder list!";
             MessageBox.Show($"Failed to load folder list: {ex.Message}\n\nPlease try refreshing the file list first.", 
"Error", MessageBoxButton.OK, MessageBoxImage.Error);
             return;
         }

         if (allFolders == null || allFolders.Count == 0)
    {
             MessageBox.Show("No folders found in the bucket.\n\nYou can create a new folder first using the 'New Folder' button.", 
        "No Folders", MessageBoxButton.OK, MessageBoxImage.Information);
      txtStatus.Text = "No folders available";
             return;
         }

      // Show folder selection dialog
  var folderDialog = new FolderSelectDialog(_currentPath, allFolders, selectedFiles.Count);
 if (folderDialog.ShowDialog() != true)
     {
     txtStatus.Text = "Move operation cancelled";
   return;
         }

   string targetFolder = folderDialog.SelectedFolder;

     // Normalize target path
          if (!string.IsNullOrEmpty(targetFolder) && !targetFolder.EndsWith("/"))
          {
       targetFolder += "/";
        }

       // Check if any files would be moved to the same path
        int samePathCount = 0;
      foreach (var file in selectedFiles)
    {
          string destKey = targetFolder + IOPath.GetFileName(file.FullPath);
     if (file.FullPath == destKey)
   {
 samePathCount++;
          }
 }

      if (samePathCount == selectedFiles.Count)
           {
   MessageBox.Show(
$"All selected files are already in the target path!\n\nCurrent path: {(string.IsNullOrEmpty(_currentPath) ? "/" : _currentPath)}\nTarget path: {(string.IsNullOrEmpty(targetFolder) ? "/" : targetFolder)}\n\nNo move needed.",
       "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
         txtStatus.Text = "No move needed";
       return;
    }

 // Show confirmation message
   string targetDisplay = string.IsNullOrEmpty(targetFolder) ? "/" : targetFolder;
   string confirmMessage = $"Are you sure you want to move {selectedFiles.Count} file(s) to '{targetDisplay}'?";
    if (samePathCount > 0)
     {
 confirmMessage += $"\n\nNote: {samePathCount} file(s) are already in the target path and will be skipped.";
    }

     var result = MessageBox.Show(confirmMessage, "Confirm Move", MessageBoxButton.YesNo, MessageBoxImage.Question);

          if (result == MessageBoxResult.Yes)
      {
    txtStatus.Text = $"Moving {selectedFiles.Count} file(s)...";
      EnableOperationButtons(false);

      try
   {
    var sourceKeys = selectedFiles.Select(f => f.FullPath).ToList();
var moveResult = await _ossService.MoveFilesAsync(sourceKeys, targetFolder);

         txtStatus.Text = $"Move complete: {moveResult.success} succeeded, {moveResult.failed} failed";

string resultMessage = $"Move complete!\nSucceeded: {moveResult.success}\nFailed: {moveResult.failed}";
          if (samePathCount > 0)
     {
     resultMessage += $"\nSkipped (already in target path): {samePathCount}";
             }
            MessageBox.Show(resultMessage, "Complete", MessageBoxButton.OK, MessageBoxImage.Information);

   LoadFiles();
         }
        catch (Exception ex)
        {
  txtStatus.Text = "Move failed!";
          MessageBox.Show($"Move failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
       finally
    {
  EnableOperationButtons(true);
      }
     }
       else
    {
        txtStatus.Text = "Move cancelled";
         }
      }
        catch (Exception ex)
         {
     txtStatus.Text = "Operation failed!";
 MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
        }

        private void UpdateSelectedCount()
        {
            if (_allFiles != null)
            {
       int checkedCount = _allFiles.Count(f => f.IsSelected);
         int selectedRowCount = dgFiles.SelectedItems.Count;

            var checkedFiles = _allFiles.Where(f => f.IsSelected).ToList();
          var selectedRows = dgFiles.SelectedItems.Cast<OssFileInfo>().ToList();
                int totalSelected = checkedFiles.Union(selectedRows).Distinct().Count();

      txtSelectedCount.Text = $"Selected: {totalSelected} (Checked:{checkedCount} + Rows:{selectedRowCount})";
    }
        }

private void LoadSavedConfig()
        {
            var config = ConfigService.LoadConfig();
      if (config != null)
   {
      txtAccessKeyId.Text = config.AccessKeyId;
             txtAccessKeySecret.Password = config.AccessKeySecret;
                txtEndpoint.Text = config.Endpoint;
      txtBucketName.Text = config.BucketName;

              txtStatus.Text = "Saved configuration loaded";
  }
        }

        private void BtnClearConfig_Click(object sender, RoutedEventArgs e)
        {
 var result = MessageBox.Show("Are you sure you want to clear saved configuration?", "Confirm",
     MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
         bool success = ConfigService.DeleteConfig();
      if (success)
         {
         txtAccessKeyId.Text = string.Empty;
       txtAccessKeySecret.Password = string.Empty;
     txtEndpoint.Text = string.Empty;
     txtBucketName.Text = string.Empty;
  chkRememberConfig.IsChecked = false;

     MessageBox.Show("Configuration cleared!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    txtStatus.Text = "Configuration cleared";
       }
          else
      {
    MessageBox.Show("Failed to clear configuration!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
            }
   }

    private void EnableOperationButtons(bool enabled)
     {
  btnRefresh.IsEnabled = enabled;
btnNewFolder.IsEnabled = enabled;
      btnUpload.IsEnabled = enabled;
    btnDownload.IsEnabled = enabled;
            btnDelete.IsEnabled = enabled;
  btnGetUrl.IsEnabled = enabled;
btnBatchMove.IsEnabled = enabled;
       cmbSortOrder.IsEnabled = enabled;
   }
    }
}
