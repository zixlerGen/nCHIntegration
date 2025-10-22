namespace nCHIntegration.Models
{
    public class DataSendOut
    {
        public DateTime Created_Datetime { get; set; }
        public String Data_Domain { get; set; }
        public String HN { get; set; }
        public String Filename { get; set; }
        public String File_Content { get; set; }
        public DateTime Send_Datetime { get; set; }
        public String Send_Status { get; set; }
        public String Send_Response { get; set; }
    }
}
