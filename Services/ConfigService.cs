using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    /// <summary>
    /// Configuration management service - responsible for saving and loading OSS configuration
    /// </summary>
public class ConfigService
    {
        // Get the directory where the application executable is located
      private static readonly string AppDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
        
        private static readonly string ConfigFilePath = Path.Combine(AppDirectory, "config.json");

     /// <summary>
        /// Save configuration to local file
      /// </summary>
        public static bool SaveConfig(OssConfig config)
        {
  try
 {
    // Serialize configuration object
           var options = new JsonSerializerOptions
      {
            WriteIndented = true, // Format JSON
         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Support Chinese characters
   };

      string jsonString = JsonSerializer.Serialize(config, options);

         // Write to file in application directory
    File.WriteAllText(ConfigFilePath, jsonString);
   
    return true;
            }
   catch
    {
      return false;
      }
    }

     /// <summary>
    /// Load configuration from local file
      /// </summary>
        public static OssConfig? LoadConfig()
        {
            try
    {
      // Check if file exists
  if (!File.Exists(ConfigFilePath))
             {
     return null;
       }

 // Read file content
    string jsonString = File.ReadAllText(ConfigFilePath);
    
  // Deserialize
       var config = JsonSerializer.Deserialize<OssConfig>(jsonString);
        
      return config;
            }
            catch
        {
          return null;
         }
     }

        /// <summary>
        /// Delete saved configuration
        /// </summary>
        public static bool DeleteConfig()
        {
   try
        {
  if (File.Exists(ConfigFilePath))
    {
   File.Delete(ConfigFilePath);
     }
       return true;
       }
   catch
      {
          return false;
  }
        }

        /// <summary>
        /// Check if saved configuration exists
    /// </summary>
        public static bool ConfigExists()
        {
        return File.Exists(ConfigFilePath);
      }

        /// <summary>
        /// Get the full path where config file will be saved
      /// </summary>
    public static string GetConfigFilePath()
        {
    return ConfigFilePath;
}
    }
}
