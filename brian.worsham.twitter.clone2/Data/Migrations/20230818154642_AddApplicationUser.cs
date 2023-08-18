using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace brian.worsham.twitter.clone2.Data.Migrations
{
    public partial class AddApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
        name: "UserName",
        table: "AspNetUsers",
        maxLength: 20,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(256)",
        oldMaxLength: 256,
        oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
