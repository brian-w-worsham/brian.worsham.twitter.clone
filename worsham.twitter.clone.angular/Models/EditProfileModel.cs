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
        public byte[] FormFile { get; set; }
        public string FileName { get; set; }

        public EditProfileModel(int userId, string userName, string bio, byte[] formFile, string fileName)
        {
            UserId = userId;
            UserName = userName;
            Bio = bio;
            FormFile = formFile;
            FileName = fileName;
        }
    }
}