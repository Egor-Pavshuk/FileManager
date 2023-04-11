using System;

namespace FileManager.Models
{
    public class TokenResult
    {
        public string Access_token { get; set; }
        public string Refresh_token { get; set; }
        public string Expires_in { get; set; }
        public string Scope { get; set; }
        public string Token_type { get; set; }
        public DateTime LastRefreshTime { get; set; }
    }
}
