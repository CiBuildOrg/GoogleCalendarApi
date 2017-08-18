namespace Mvc.Server.ViewModels
{
    public class Jwt
    {
        public string Audience { get; set; }
        public string Authority { get; set; }
        public string SecretKey { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int IdentityTokenLifetime { get; set; }
        public int RefreshTokenLifetime { get; set; }
    }
}