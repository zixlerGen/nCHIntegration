namespace nCHIntegration.ViewModels
{
    public class HL7MessageVM
    {
        public string? hl7ADT { get; set; }
        public string? hl7ORM { get; set; }
        public List<string>? hl7ORU { get; set; }

        public List<HL7Message> hl7Message { get; set; } = new List<HL7Message>();
    }

    public class HL7Message
    {
        public string? MessageType { get; set; }
        public string? MessageName { get; set; }
        public string? MessageID { get; set; }
        public string? MessageContent { get; set; }
        public string? FilePath { get; set; }
        public string? SFTPSendOutTime { get; set; }
        public string? SFTPSendOutStatus { get; set; }
    }
}
