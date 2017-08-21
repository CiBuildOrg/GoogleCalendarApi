using Mvc.Server.ViewModels;

namespace Mvc.Server.Options
{
    public class AppOptions
    {
        public Application Application { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public Jwt Jwt { get; set; }
        public Auth Auth { get; set; }
    }
}
