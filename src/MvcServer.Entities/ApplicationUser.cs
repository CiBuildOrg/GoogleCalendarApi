using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace MvcServer.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ApplicationUser : IdentityUser
    {
      
    }
}
