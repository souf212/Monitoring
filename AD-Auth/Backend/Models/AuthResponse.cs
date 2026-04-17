namespace KtcWeb.Models
{
    public class AuthResponse
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;    
        public DateTime Expiration { get; set; }              
    }
}