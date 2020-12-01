using System;
using System.Collections.Generic;
using System.Text;

namespace GitBackupTool
{
    public class ForkInfo
    {
        public string Id { get; set; }

        public string SelfToken { get; set; }

        public string SelfUserName { get; set; }

        public string TargetUserName { get; set; }
    }
}
