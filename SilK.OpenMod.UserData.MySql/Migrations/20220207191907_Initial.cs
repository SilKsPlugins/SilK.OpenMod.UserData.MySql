using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SilK.OpenMod.UserData.MySql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenModUserData_Users",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    LastDisplayName = table.Column<string>(nullable: true),
                    FirstSeen = table.Column<DateTime>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenModUserData_Users", x => new { x.Id, x.Type });
                });

            migrationBuilder.CreateTable(
                name: "OpenModUserData_UserBans",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    ExpireDate = table.Column<DateTime>(nullable: true),
                    InstigatorType = table.Column<string>(nullable: true),
                    InstigatorId = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenModUserData_UserBans", x => new { x.Id, x.Type });
                    table.ForeignKey(
                        name: "FK_OpenModUserData_UserBans_OpenModUserData_Users_Id_Type",
                        columns: x => new { x.Id, x.Type },
                        principalTable: "OpenModUserData_Users",
                        principalColumns: new[] { "Id", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenModUserData_UserDataObjects",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Object = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenModUserData_UserDataObjects", x => new { x.Id, x.Type, x.Key });
                    table.ForeignKey(
                        name: "FK_OpenModUserData_UserDataObjects_OpenModUserData_Users_Id_Type",
                        columns: x => new { x.Id, x.Type },
                        principalTable: "OpenModUserData_Users",
                        principalColumns: new[] { "Id", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenModUserData_UserPermissions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Permission = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenModUserData_UserPermissions", x => new { x.Id, x.Type, x.Permission });
                    table.ForeignKey(
                        name: "FK_OpenModUserData_UserPermissions_OpenModUserData_Users_Id_Type",
                        columns: x => new { x.Id, x.Type },
                        principalTable: "OpenModUserData_Users",
                        principalColumns: new[] { "Id", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenModUserData_UserRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Role = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenModUserData_UserRoles", x => new { x.Id, x.Type, x.Role });
                    table.ForeignKey(
                        name: "FK_OpenModUserData_UserRoles_OpenModUserData_Users_Id_Type",
                        columns: x => new { x.Id, x.Type },
                        principalTable: "OpenModUserData_Users",
                        principalColumns: new[] { "Id", "Type" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenModUserData_UserBans");

            migrationBuilder.DropTable(
                name: "OpenModUserData_UserDataObjects");

            migrationBuilder.DropTable(
                name: "OpenModUserData_UserPermissions");

            migrationBuilder.DropTable(
                name: "OpenModUserData_UserRoles");

            migrationBuilder.DropTable(
                name: "OpenModUserData_Users");
        }
    }
}
