using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbraco.Cms.Persistence.EFCore.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOpenIddictToV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "umbracoOpenIddictApplications",
                newName: "ClientType");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                table: "umbracoOpenIddictApplications",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonWebKeySet",
                table: "umbracoOpenIddictApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "umbracoOpenIddictApplications",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationType",
                table: "umbracoOpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "JsonWebKeySet",
                table: "umbracoOpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "umbracoOpenIddictApplications");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "umbracoOpenIddictApplications",
                newName: "Type");
        }
    }
}
