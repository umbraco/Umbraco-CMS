using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaryDtos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: Tables are created by NPoco. This migration exists only to update the EF Core model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: Tables are managed by NPoco.
        }
    }
}
