using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace worsham.twitter.clone.angular.Models
{
    public class EditProfileModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}