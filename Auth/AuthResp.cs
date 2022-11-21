namespace MinimalAPI.Auth
{
    public class AuthResp
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string UserName { get; set; }
    }
}
