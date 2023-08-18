using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace brian.worsham.twitter.clone2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20)]
        [Required]
        public override string UserName { get; set; }
    }
}
