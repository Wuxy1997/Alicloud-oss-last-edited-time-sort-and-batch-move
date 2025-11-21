using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WpfApp1
{
  /// <summary>
    /// Folder selection dialog
    /// </summary>
    public partial class FolderSelectDialog : Window
    {
        /// <summary>
        /// Selected folder path
        /// </summary>
 public string SelectedFolder { get; private set; } = "";

        private readonly string _currentPath;
        private readonly List<FolderItem> _folders;

        public FolderSelectDialog(string currentPath, List<string> allFolders, int fileCount)
        {
       InitializeComponent();

  _currentPath = currentPath;
      _folders = new List<FolderItem>();

       // Display current path
            txtCurrentPath.Text = string.IsNullOrEmpty(currentPath) ? "/" : currentPath;
  
            // Display file count
     txtFileCount.Text = $"Moving {fileCount} file(s)";

     // Build folder list
            BuildFolderList(allFolders);
        }

        private void BuildFolderList(List<string> allFolders)
   {
      // Add root directory
            _folders.Add(new FolderItem
       {
           Path = "",
      DisplayPath = "/ (Root)",
    IsRoot = true
  });

 // Add other folders
 foreach (var folder in allFolders.OrderBy(f => f))
        {
    // Skip current folder
     if (folder == _currentPath)
          continue;

       _folders.Add(new FolderItem
    {
 Path = folder,
          DisplayPath = folder,
  IsRoot = false
    });
      }

            // If no other folders, add placeholder
          if (_folders.Count == 1)
       {
          _folders.Add(new FolderItem
       {
     Path = null,
        DisplayPath = "(No other folders available)",
          IsRoot = false
            });
            }

  lstFolders.ItemsSource = _folders;

        // Auto-select first item if current path exists
      if (!string.IsNullOrEmpty(_currentPath) && _folders.Count > 0)
        {
      lstFolders.SelectedIndex = 0;
      }
        }

        private void LstFolders_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        if (lstFolders.SelectedItem is FolderItem selectedItem)
            {
     // Disable OK button if placeholder is selected
         btnOK.IsEnabled = selectedItem.Path != null;
      
  if (selectedItem.Path != null)
          {
        SelectedFolder = selectedItem.Path;
     }
 }
     }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
    if (lstFolders.SelectedItem is FolderItem selectedItem && selectedItem.Path != null)
            {
   SelectedFolder = selectedItem.Path;
  DialogResult = true;
     Close();
       }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
   {
            DialogResult = false;
            Close();
   }

        /// <summary>
        /// Folder item model
/// </summary>
        private class FolderItem
        {
            public string? Path { get; set; }
            public string DisplayPath { get; set; } = "";
            public bool IsRoot { get; set; }
        }
    }
}
