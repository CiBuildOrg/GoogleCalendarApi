using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcServer.Entities;
using OpenIddict.Models;

namespace Mvc.Server.Database
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }
        
        public DbSet<OpenIddictApplication> OpenIdApplications { get; set; }
        public DbSet<OpenIddictToken> OpenIddictTokens { get; set; }
    }
}
