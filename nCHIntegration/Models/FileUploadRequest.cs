namespace nCHIntegration.Models
{
    public class FileUploadRequest
    {
        public string LocalFilePath { get; set; }
        public string RemoteFileName { get; set; }
        public string SftpHost { get; set; }
        public int SftpPort { get; set; } = 22;
        public string SftpUsername { get; set; }    
        public string SftpPassword { get; set; }
        public string RemoteDirectory { get; set; } 
        public string CorrelationID { get; set; }
    }
}
