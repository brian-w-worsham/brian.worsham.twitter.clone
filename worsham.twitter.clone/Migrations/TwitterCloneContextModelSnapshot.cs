﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using worsham.twitter.clone.Models;
using worsham.twitter.clone.Models.EntityModels;

#nullable disable

namespace worsham.twitter.clone.Migrations
{
    [DbContext(typeof(TwitterCloneContext))]
    partial class TwitterCloneContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.21")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("worsham.twitter.clone.Models.Comments", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CommenterId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(280)
                        .IsUnicode(false)
                        .HasColumnType("varchar(280)");

                    b.Property<int>("OriginalTweetId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CommenterId");

                    b.HasIndex("OriginalTweetId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Follows", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("FollowedUserId")
                        .HasColumnType("int");

                    b.Property<int>("FollowerUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FollowedUserId");

                    b.HasIndex("FollowerUserId");

                    b.ToTable("Follows");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Likes", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("LikedTweetId")
                        .HasColumnType("int");

                    b.Property<int>("UserThatLikedTweetId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LikedTweetId");

                    b.ToTable("Likes");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.ReTweets", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("OriginalTweetId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ReTweetCreationDateTime")
                        .HasColumnType("datetime");

                    b.Property<int>("RetweeterId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OriginalTweetId");

                    b.HasIndex("RetweeterId");

                    b.ToTable("ReTweets");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Tweets", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(280)
                        .IsUnicode(false)
                        .HasColumnType("varchar(280)");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("datetime");

                    b.Property<int>("TweeterId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TweeterId");

                    b.ToTable("Tweets");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Bio")
                        .HasMaxLength(160)
                        .IsUnicode(false)
                        .HasColumnType("varchar(160)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .IsUnicode(false)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(12)
                        .IsUnicode(false)
                        .HasColumnType("varchar(12)");

                    b.Property<string>("ProfilePictureUrl")
                        .HasMaxLength(300)
                        .IsUnicode(false)
                        .HasColumnType("varchar(300)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .IsUnicode(false)
                        .HasColumnType("varchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Comments", b =>
                {
                    b.HasOne("worsham.twitter.clone.Models.Users", "Commenter")
                        .WithMany("Comments")
                        .HasForeignKey("CommenterId")
                        .IsRequired()
                        .HasConstraintName("FK_Comments_Users");

                    b.HasOne("worsham.twitter.clone.Models.Tweets", "OriginalTweet")
                        .WithMany("Comments")
                        .HasForeignKey("OriginalTweetId")
                        .IsRequired()
                        .HasConstraintName("FK_Comments_Tweets");

                    b.Navigation("Commenter");

                    b.Navigation("OriginalTweet");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Follows", b =>
                {
                    b.HasOne("worsham.twitter.clone.Models.Users", "FollowedUser")
                        .WithMany("FollowsFollowedUser")
                        .HasForeignKey("FollowedUserId")
                        .IsRequired()
                        .HasConstraintName("FK_Follows_Users");

                    b.HasOne("worsham.twitter.clone.Models.Users", "FollowerUser")
                        .WithMany("FollowsFollowerUser")
                        .HasForeignKey("FollowerUserId")
                        .IsRequired()
                        .HasConstraintName("FK_FollowerUserId");

                    b.Navigation("FollowedUser");

                    b.Navigation("FollowerUser");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Likes", b =>
                {
                    b.HasOne("worsham.twitter.clone.Models.Users", "UserThatLikedTweet")
                        .WithOne("Likes")
                        .HasForeignKey("worsham.twitter.clone.Models.Likes", "Id")
                        .IsRequired()
                        .HasConstraintName("FK_Likes_Users");

                    b.HasOne("worsham.twitter.clone.Models.Tweets", "LikedTweet")
                        .WithMany("Likes")
                        .HasForeignKey("LikedTweetId")
                        .IsRequired()
                        .HasConstraintName("FK_Likes_Tweets");

                    b.Navigation("LikedTweet");

                    b.Navigation("UserThatLikedTweet");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.ReTweets", b =>
                {
                    b.HasOne("worsham.twitter.clone.Models.Tweets", "OriginalTweet")
                        .WithMany("ReTweets")
                        .HasForeignKey("OriginalTweetId")
                        .IsRequired()
                        .HasConstraintName("FK_ReTweets_Tweets");

                    b.HasOne("worsham.twitter.clone.Models.Users", "Retweeter")
                        .WithMany("ReTweets")
                        .HasForeignKey("RetweeterId")
                        .IsRequired()
                        .HasConstraintName("FK_ReTweets_Users");

                    b.Navigation("OriginalTweet");

                    b.Navigation("Retweeter");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Tweets", b =>
                {
                    b.HasOne("worsham.twitter.clone.Models.Users", "Tweeter")
                        .WithMany("Tweets")
                        .HasForeignKey("TweeterId")
                        .IsRequired()
                        .HasConstraintName("FK_Tweets_Users");

                    b.Navigation("Tweeter");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Tweets", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Likes");

                    b.Navigation("ReTweets");
                });

            modelBuilder.Entity("worsham.twitter.clone.Models.Users", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("FollowsFollowedUser");

                    b.Navigation("FollowsFollowerUser");

                    b.Navigation("Likes");

                    b.Navigation("ReTweets");

                    b.Navigation("Tweets");
                });
#pragma warning restore 612, 618
        }
    }
}
