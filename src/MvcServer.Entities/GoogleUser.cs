using System;
using System.ComponentModel.DataAnnotations;

namespace MvcServer.Entities
{
    public class GoogleUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
