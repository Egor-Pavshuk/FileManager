namespace FileManager.Models
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
    }
}
