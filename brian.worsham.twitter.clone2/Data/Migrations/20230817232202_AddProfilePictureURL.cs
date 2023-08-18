using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace brian.worsham.twitter.clone2.Data.Migrations
{
    public partial class AddProfilePictureURL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "ProfilePictureURL", table: "AspNetUsers", maxLength: 255, nullable: true);  // Update as needed based on your requirements
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
