using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpServiceNCH
{
    public class SftpSettings
    {
        public string sftpHost { get; set; }
        public int sftpport { get; set; }
        public string sftpusername { get; set; }
        public string sftpRemotePath { get; set; }
        public string sftpPrivateKeyPath { get; set; }
        public string sftpLocalPath { get; set; }
    }
}
