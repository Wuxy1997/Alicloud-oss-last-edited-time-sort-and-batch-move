using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services
{
  /// <summary>
    /// Aliyun OSS service class
    /// </summary>
    public class OssService
    {
        private OssClient? _client;
    private string? _bucketName;

        /// <summary>
        /// Initialize OSS client
        /// </summary>
        public bool Initialize(OssConfig config)
        {
  try
     {
                _client = new OssClient(config.Endpoint, config.AccessKeyId, config.AccessKeySecret);
     _bucketName = config.BucketName;
        
      // Test connection
           var bucketExists = _client.DoesBucketExist(_bucketName);
       return bucketExists;
          }
  catch
      {
          return false;
   }
        }

   /// <summary>
        /// Get all files list
     /// </summary>
        public List<OssFileInfo> ListFiles(string prefix = "")
        {
            if (_client == null || string.IsNullOrEmpty(_bucketName))
          throw new InvalidOperationException("OSS client not initialized");

  var files = new List<OssFileInfo>();
            
      try
            {
       ObjectListing result;
 string nextMarker = string.Empty;

      do
         {
              var listObjectsRequest = new ListObjectsRequest(_bucketName)
 {
    Marker = nextMarker,
     MaxKeys = 100,
     Prefix = prefix,
     Delimiter = "/" // Add delimiter to list only current directory
             };

    result = _client.ListObjects(listObjectsRequest);

          // Add folders
       foreach (var commonPrefix in result.CommonPrefixes)
              {
      string folderName = commonPrefix.TrimEnd('/');
 if (!string.IsNullOrEmpty(prefix))
       {
                 folderName = folderName.Substring(prefix.Length);
             }

  files.Add(new OssFileInfo
     {
           FileName = folderName,
            FullPath = commonPrefix,
 Size = 0,
     LastModified = DateTime.MinValue,
      FileType = "[Folder]",
    ETag = "",
         IsFolder = true
          });
   }

          // Add files
        foreach (var summary in result.ObjectSummaries)
       {
      // Skip folder markers
       if (summary.Key.EndsWith("/"))
     continue;

         // Only show files in current directory
               string fileName = summary.Key;
       if (!string.IsNullOrEmpty(prefix))
                {
 fileName = fileName.Substring(prefix.Length);
            }

      // If filename contains /, it's a file in subdirectory, skip it
              if (fileName.Contains("/"))
                   continue;

 files.Add(new OssFileInfo
           {
    FileName = fileName,
   FullPath = summary.Key,
         Size = summary.Size,
          LastModified = summary.LastModified,
    FileType = Path.GetExtension(summary.Key),
      ETag = summary.ETag,
       IsFolder = false
                });
         }

    nextMarker = result.NextMarker;
     } while (result.IsTruncated);
         }
  catch (Exception ex)
  {
         throw new Exception($"Failed to get file list: {ex.Message}", ex);
            }

        return files;
        }

     /// <summary>
        /// Upload file
        /// </summary>
        public async Task<bool> UploadFileAsync(string localFilePath, string ossKey)
        {
 if (_client == null || string.IsNullOrEmpty(_bucketName))
      throw new InvalidOperationException("OSS client not initialized");

   return await Task.Run(() =>
         {
      try
  {
        _client.PutObject(_bucketName, ossKey, localFilePath);
       return true;
                }
        catch
    {
     return false;
        }
          });
  }

 /// <summary>
        /// Download file
        /// </summary>
        public async Task<bool> DownloadFileAsync(string ossKey, string localFilePath)
        {
 if (_client == null || string.IsNullOrEmpty(_bucketName))
         throw new InvalidOperationException("OSS client not initialized");

    return await Task.Run(() =>
{
                try
     {
          var obj = _client.GetObject(_bucketName, ossKey);
  
        using (var requestStream = obj.Content)
        using (var fileStream = File.Create(localFilePath))
{
      requestStream.CopyTo(fileStream);
   }
   
    return true;
          }
                catch
              {
                 return false;
          }
   });
        }

      /// <summary>
        /// Delete file
        /// </summary>
      public bool DeleteFile(string ossKey)
     {
   if (_client == null || string.IsNullOrEmpty(_bucketName))
     throw new InvalidOperationException("OSS client not initialized");

         try
            {
       _client.DeleteObject(_bucketName, ossKey);
    return true;
       }
    catch
      {
     return false;
        }
        }

        /// <summary>
        /// Get file URL (valid for 1 hour)
        /// </summary>
    public string GetFileUrl(string ossKey)
        {
  if (_client == null || string.IsNullOrEmpty(_bucketName))
              throw new InvalidOperationException("OSS client not initialized");

 try
            {
                var req = new GeneratePresignedUriRequest(_bucketName, ossKey, SignHttpMethod.Get)
          {
          Expiration = DateTime.Now.AddHours(1)
     };

   var uri = _client.GeneratePresignedUri(req);
     return uri.ToString();
            }
            catch
       {
              return string.Empty;
  }
  }

  /// <summary>
        /// Copy file to new path
   /// </summary>
        public bool CopyFile(string sourceKey, string destKey)
        {
        if (_client == null || string.IsNullOrEmpty(_bucketName))
  throw new InvalidOperationException("OSS client not initialized");

          try
 {
    var request = new CopyObjectRequest(_bucketName, sourceKey, _bucketName, destKey);
   _client.CopyObject(request);
        return true;
            }
    catch
  {
          return false;
       }
        }

  /// <summary>
        /// Move file to new path (copy then delete source file)
        /// </summary>
        public async Task<bool> MoveFileAsync(string sourceKey, string destKey)
        {
            if (_client == null || string.IsNullOrEmpty(_bucketName))
                throw new InvalidOperationException("OSS client not initialized");

            return await Task.Run(() =>
       {
          try
       {
          // Copy first
             var copyRequest = new CopyObjectRequest(_bucketName, sourceKey, _bucketName, destKey);
_client.CopyObject(copyRequest);

    // Then delete source file
              _client.DeleteObject(_bucketName, sourceKey);
    return true;
  }
   catch
                {
     return false;
     }
  });
        }

        /// <summary>
        /// Batch move files
        /// </summary>
        public async Task<(int success, int failed)> MoveFilesAsync(List<string> sourceKeys, string targetFolder)
     {
   int successCount = 0;
          int failedCount = 0;

            // Normalize target folder path
     targetFolder = targetFolder?.Trim() ?? "";
   if (!string.IsNullOrEmpty(targetFolder) && !targetFolder.EndsWith("/"))
{
         targetFolder += "/";
        }

  foreach (var sourceKey in sourceKeys)
       {
     try
          {
    // Build target path
      string fileName = Path.GetFileName(sourceKey);
            string destKey = targetFolder + fileName;

  // ⚠️ Check if source and target paths are the same
            if (sourceKey == destKey)
        {
    // Skip, same path doesn't need moving
           failedCount++;
  continue;
       }

    bool success = await MoveFileAsync(sourceKey, destKey);
            if (success)
          successCount++;
   else
   failedCount++;
       }
     catch
    {
         failedCount++;
      }
      }

       return (successCount, failedCount);
        }

        /// <summary>
        /// Batch delete files
   /// </summary>
        public async Task<(int success, int failed)> DeleteFilesAsync(List<string> ossKeys)
        {
      int successCount = 0;
   int failedCount = 0;

            foreach (var key in ossKeys)
     {
try
    {
       _client?.DeleteObject(_bucketName, key);
           successCount++;
                }
       catch
         {
               failedCount++;
        }
 }

    return await Task.FromResult((successCount, failedCount));
}

   /// <summary>
        /// Create folder
        /// </summary>
        public bool CreateFolder(string folderPath)
        {
            if (_client == null || string.IsNullOrEmpty(_bucketName))
                throw new InvalidOperationException("OSS client not initialized");

            try
  {
      // Ensure folder path ends with /
 if (!folderPath.EndsWith("/"))
     {
         folderPath += "/";
           }

     // Create an empty object as folder marker
             using (var ms = new MemoryStream())
                {
             _client.PutObject(_bucketName, folderPath, ms);
         }

                return true;
            }
          catch
  {
     return false;
            }
        }

/// <summary>
   /// Get all folders list (recursive)
 /// </summary>
      public List<string> GetAllFolders()
     {
   if (_client == null || string.IsNullOrEmpty(_bucketName))
   throw new InvalidOperationException("OSS client not initialized");

   var folders = new HashSet<string>(); // Use HashSet to avoid duplicates

   try
      {
    // Method 1: Get folders from file paths (files inside folders)
      ObjectListing result;
  string nextMarker = string.Empty;
      int objectCount = 0;
         int folderMarkerCount = 0;

do
      {
    var listObjectsRequest = new ListObjectsRequest(_bucketName)
   {
    Marker = nextMarker,
   MaxKeys = 1000
     };

    result = _client.ListObjects(listObjectsRequest);
 objectCount += result.ObjectSummaries.Count();

   // Extract folder paths from all object keys
   foreach (var summary in result.ObjectSummaries)
    {
    string key = summary.Key;
    
        // If it's a folder marker (ends with /), add it directly
            if (key.EndsWith("/"))
{
      folders.Add(key);
       folderMarkerCount++;
     }
            
   // Extract all parent folder levels from the key
   int lastSlashIndex = key.LastIndexOf('/');
   while (lastSlashIndex > 0)
 {
    string folderPath = key.Substring(0, lastSlashIndex + 1);
   folders.Add(folderPath);
     
 // Move to parent folder
     key = key.Substring(0, lastSlashIndex);
     lastSlashIndex = key.LastIndexOf('/');
     }
    }

   nextMarker = result.NextMarker;
   } while (result.IsTruncated);
         
      // Method 2: Also scan using delimiter to catch any folders we might have missed
         try
         {
             nextMarker = string.Empty;
      do
  {
       var delimiterRequest = new ListObjectsRequest(_bucketName)
    {
         Marker = nextMarker,
          MaxKeys = 1000,
       Delimiter = "/"
     };
       
         var delimiterResult = _client.ListObjects(delimiterRequest);
       
          foreach (var commonPrefix in delimiterResult.CommonPrefixes)
    {
     folders.Add(commonPrefix);
         }
    
     nextMarker = delimiterResult.NextMarker;
    } while (result.IsTruncated);
    }
  catch
   {
         // Ignore errors from delimiter scan
            }
  
       // Debug: Log the results
 System.Diagnostics.Debug.WriteLine($"GetAllFolders: Scanned {objectCount} objects, found {folderMarkerCount} folder markers, extracted {folders.Count} total folders");
       foreach (var folder in folders.OrderBy(f => f))
    {
     System.Diagnostics.Debug.WriteLine($"  - {folder}");
      }
       }
     catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"GetAllFolders error: {ex.Message}");
   throw new Exception($"Failed to get folder list: {ex.Message}", ex);
   }

   return folders.OrderBy(f => f).ToList();
 }

        /// <summary>
     /// Recursively get subfolders (deprecated - using simpler approach in GetAllFolders now)
      /// </summary>
   private void GetSubFolders(string prefix, List<string> folders)
 {
   try
{
         var listObjectsRequest = new ListObjectsRequest(_bucketName)
   {
        Prefix = prefix,
        MaxKeys = 1000,
  Delimiter = "/"
          };

    var result = _client.ListObjects(listObjectsRequest);

         foreach (var commonPrefix in result.CommonPrefixes)
       {
    if (!folders.Contains(commonPrefix))
           {
     folders.Add(commonPrefix);
         GetSubFolders(commonPrefix, folders);
  }
     }
 }
     catch
       {
// Ignore errors, continue processing other folders
         }
        }

/// <summary>
        /// Delete folder (including all files inside)
/// </summary>
        public async Task<bool> DeleteFolderAsync(string folderPath)
        {
  if (_client == null || string.IsNullOrEmpty(_bucketName))
    throw new InvalidOperationException("OSS client not initialized");

            return await Task.Run(() =>
     {
                try
        {
  // Ensure folder path ends with /
       if (!folderPath.EndsWith("/"))
           {
    folderPath += "/";
    }

      // List all files in folder
          var listResult = _client.ListObjects(new ListObjectsRequest(_bucketName)
    {
    Prefix = folderPath
  });

        // Delete all files
        foreach (var obj in listResult.ObjectSummaries)
             {
           _client.DeleteObject(_bucketName, obj.Key);
     }

           return true;
           }
      catch
         {
      return false;
       }
        });
        }
    }
}
