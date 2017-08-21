namespace Mvc.Server.Options
{
    public class Auth
    {
        public string AuthorizeEndpoint { get; set; }
        public string LogoutEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
    }
}