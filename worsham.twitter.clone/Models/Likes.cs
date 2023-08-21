﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace worsham.twitter.clone.Models
{
    public partial class Likes
    {
        [Key]
        public int Id { get; set; }
        public int LikedTweetId { get; set; }
        public int UserThatLikedTweetId { get; set; }

        [ForeignKey("UserThatLikedTweetId")]
        [InverseProperty("Likes")]
        public virtual Users UserThatLikedTweet { get; set; }

        [ForeignKey("LikedTweetId")]
        [InverseProperty("Likes")]
        public virtual Tweets LikedTweet { get; set; }
    }
}