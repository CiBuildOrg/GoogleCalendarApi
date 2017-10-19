namespace Mvc.Server.DataObjects.Configuration
{
    public class ApiSettings
    {
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool RequireHttps { get; set; }
    }
}