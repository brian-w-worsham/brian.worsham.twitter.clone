﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.Utils;

namespace worsham.twitter.clone.Models
{
    public partial class Users
    {
        public Users()
        {
            Comments = new HashSet<Comments>();
            FollowsFollowedUser = new HashSet<Follows>();
            FollowsFollowerUser = new HashSet<Follows>();
            ReTweets = new HashSet<ReTweets>();
            Tweets = new HashSet<Tweets>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        [Unicode(false)]
        public string UserName { get; set; }
        [Required]
        [StringLength(200)]
        [Unicode(false)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        [Required]
        [StringLength(12)]
        [Unicode(false)]
        [PasswordValidation(8, 12)]
        public string Password { get; set; }
        [StringLength(160)]
        [Unicode(false)]
        public string Bio { get; set; }
        [StringLength(300)]
        [Unicode(false)]
        public string ProfilePictureUrl { get; set; }


        [InverseProperty("UserThatLikedTweet")]
        public virtual Likes Likes { get; set; }
        [InverseProperty("Commenter")]
        public virtual ICollection<Comments> Comments { get; set; }
        [InverseProperty("FollowedUser")]
        public virtual ICollection<Follows> FollowsFollowedUser { get; set; }
        [InverseProperty("FollowerUser")]
        public virtual ICollection<Follows> FollowsFollowerUser { get; set; }
        [InverseProperty("Retweeter")]
        public virtual ICollection<ReTweets> ReTweets { get; set; }
        [InverseProperty("Tweeter")]
        public virtual ICollection<Tweets> Tweets { get; set; }
    }
}