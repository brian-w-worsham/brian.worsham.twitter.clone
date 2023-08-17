﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace brian.worsham.twitter.clone2.Models
{
    public partial class ReTweets
    {
        [Key]
        public int Id { get; set; }
        public int OriginalTweetId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ReTweetCreationDateTime { get; set; }

        [ForeignKey("OriginalTweetId")]
        [InverseProperty("ReTweets")]
        public virtual Tweets OriginalTweet { get; set; }
    }
}