using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace worsham.twitter.clone.angular.Models
{
    public class PostModel
    {
        [Display(Name = "Tweet")]
        [Required]
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Tweet must be between 1 and 280 characters.")]
        [Unicode(false)]
        public string Content { get; set; }
    }
}
