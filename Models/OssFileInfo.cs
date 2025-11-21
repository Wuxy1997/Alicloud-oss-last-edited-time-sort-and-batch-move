using System;
using System.ComponentModel;

namespace WpfApp1.Models
{
    /// <summary>
    /// OSS file information model
    /// </summary>
    public class OssFileInfo : INotifyPropertyChanged
    {
        private bool _isSelected;

        /// <summary>
        /// Whether the item is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File full path (Key)
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// File size (bytes)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Formatted file size
        /// </summary>
        public string FormattedSize => IsFolder ? "[Folder]" : FormatFileSize(Size);

        /// <summary>
        /// Last modified time
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Formatted last modified time
        /// </summary>
        public string FormattedLastModified => LastModified.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// File type
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// ETag
        /// </summary>
        public string ETag { get; set; } = string.Empty;

        /// <summary>
        /// Whether it's a folder
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Display icon (using Unicode characters)
        /// </summary>
        public string Icon => IsFolder ? "📁" : "📄";

        /// <summary>
        /// Display text icon (alternative)
        /// </summary>
        public string TextIcon => IsFolder ? "[ 📂 ]" : "[ 📃 ]";

        /// <summary>
        /// Format file size
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
