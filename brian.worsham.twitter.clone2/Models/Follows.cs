﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace brian.worsham.twitter.clone2.Models
{
    public partial class Follows
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(450)]
        public string FollowedUserId { get; set; }
        [Required]
        [StringLength(450)]
        public string FollowerUserId { get; set; }

        [ForeignKey("FollowedUserId")]
        [InverseProperty("FollowsFollowedUser")]
        public virtual AspNetUsers FollowedUser { get; set; }
        [ForeignKey("FollowerUserId")]
        [InverseProperty("FollowsFollowerUser")]
        public virtual AspNetUsers FollowerUser { get; set; }
    }
}