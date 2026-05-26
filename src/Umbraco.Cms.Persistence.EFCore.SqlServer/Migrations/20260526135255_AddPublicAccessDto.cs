using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicAccessDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "umbracoAccess",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nodeId = table.Column<int>(type: "int", nullable: false),
                    loginNodeId = table.Column<int>(type: "int", nullable: false),
                    noAccessNodeId = table.Column<int>(type: "int", nullable: false),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umbracoAccess", x => x.id);
                    table.ForeignKey(
                        name: "FK_umbracoAccess_umbracoNode_id",
                        column: x => x.nodeId,
                        principalTable: "umbracoNode",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_umbracoAccess_umbracoNode_id1",
                        column: x => x.loginNodeId,
                        principalTable: "umbracoNode",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_umbracoAccess_umbracoNode_id2",
                        column: x => x.noAccessNodeId,
                        principalTable: "umbracoNode",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "umbracoAccessRule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    accessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ruleValue = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ruleType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_umbracoAccessRule", x => x.id);
                    table.ForeignKey(
                        name: "FK_umbracoAccessRule_umbracoAccess_accessId",
                        column: x => x.accessId,
                        principalTable: "umbracoAccess",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_umbracoAccess_loginNodeId",
                table: "umbracoAccess",
                column: "loginNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_umbracoAccess_noAccessNodeId",
                table: "umbracoAccess",
                column: "noAccessNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_umbracoAccess_nodeId",
                table: "umbracoAccess",
                column: "nodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_umbracoAccessRule",
                table: "umbracoAccessRule",
                columns: new[] { "ruleValue", "ruleType", "accessId" },
                unique: true,
                filter: "[ruleValue] IS NOT NULL AND [ruleType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_umbracoAccessRule_accessId",
                table: "umbracoAccessRule",
                column: "accessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "umbracoAccessRule");

            migrationBuilder.DropTable(
                name: "umbracoAccess");
        }
    }
}
