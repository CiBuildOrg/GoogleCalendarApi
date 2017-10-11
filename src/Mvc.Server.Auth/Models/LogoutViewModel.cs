using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.Server.Auth.Models
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}