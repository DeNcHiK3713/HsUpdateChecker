using System;
using System.Collections.Generic;
using System.Text;

namespace HsUpdateChecker
{
    public class AppSettings
    {
        public string TgToken { get; set; }
        public long TgChatId { get; set; }
        public ulong DsWebhookId { get; set; }
        public string DsWebhookToken { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SavePath { get; set; }
    }
}
