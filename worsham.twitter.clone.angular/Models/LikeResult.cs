using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace worsham.twitter.clone.angular.Models
{
    public class LikeResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
    }
}