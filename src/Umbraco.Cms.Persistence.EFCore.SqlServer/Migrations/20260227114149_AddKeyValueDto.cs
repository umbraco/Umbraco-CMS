using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyValueDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "umbracoKeyValue",
                columns: table => new
                {
                    key = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umbracoKeyValue", x => x.key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "umbracoKeyValue");
        }
    }
}
