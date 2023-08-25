﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace worsham.twitter.clone.Models.EntityModels
{
    public partial class Comments
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(280)]
        [Unicode(false)]
        public string Content { get; set; }
        public int OriginalTweetId { get; set; }
        public int CommenterId { get; set; }

        [ForeignKey("CommenterId")]
        [InverseProperty("Comments")]
        public virtual Users Commenter { get; set; }
        [ForeignKey("OriginalTweetId")]
        [InverseProperty("Comments")]
        public virtual Tweets OriginalTweet { get; set; }
    }
}