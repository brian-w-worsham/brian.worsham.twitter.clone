﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace b.worsham.twitter.clone.Models
{
    public partial class Follows
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        public int FollowedUserId { get; set; }
        public int FollowerUserId { get; set; }

        [ForeignKey("FollowedUserId")]
        [InverseProperty("FollowsFollowedUser")]
        public virtual Users FollowedUser { get; set; }
        [ForeignKey("FollowerUserId")]
        [InverseProperty("FollowsFollowerUser")]
        public virtual Users FollowerUser { get; set; }
    }
}