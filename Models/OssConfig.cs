namespace WpfApp1.Models
{
  /// <summary>
    /// OSS configuration model
  /// </summary>
    public class OssConfig
    {
        /// <summary>
  /// Access Key ID
        /// </summary>
  public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// Access Key Secret
        /// </summary>
        public string AccessKeySecret { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint (e.g. oss-cn-hangzhou.aliyuncs.com)
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Bucket name
        /// </summary>
    public string BucketName { get; set; } = string.Empty;
    }
}
