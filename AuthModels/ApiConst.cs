using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MinimalAPI.AuthModels
{
    public class ApiConst
    {
        public string Issuer { get; set; }
        public  string Audience { get; set; }
        public  string key { get; set; }

        public  string AuthSchemes { get; set; }
        public int expiresIn { get; set; }
    }
}
