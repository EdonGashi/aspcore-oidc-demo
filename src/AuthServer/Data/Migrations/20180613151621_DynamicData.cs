using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Data.Migrations
{
    public partial class DynamicData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicDataApplicationProperties",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Flags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicDataApplicationProperties", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "DynamicDataUserApplicationProperties",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Flags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicDataUserApplicationProperties", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_DynamicDataUserApplicationProperties_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicDataUserClientProperties",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Flags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicDataUserClientProperties", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_DynamicDataUserClientProperties_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicDataUserPersonalProperties",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Flags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicDataUserPersonalProperties", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_DynamicDataUserPersonalProperties_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicDataApplicationProperties");

            migrationBuilder.DropTable(
                name: "DynamicDataUserApplicationProperties");

            migrationBuilder.DropTable(
                name: "DynamicDataUserClientProperties");

            migrationBuilder.DropTable(
                name: "DynamicDataUserPersonalProperties");
        }
    }
}
