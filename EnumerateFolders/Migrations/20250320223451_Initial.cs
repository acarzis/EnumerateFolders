using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EnumerateFolders.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Extensions = table.Column<string>(nullable: true),
                    FolderLocations = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Drives",
                columns: table => new
                {
                    LogicalDrive = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ScanPriority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drives", x => x.LogicalDrive);
                });

            migrationBuilder.CreateTable(
                name: "FolderExclusions",
                columns: table => new
                {
                    FullPath = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderExclusions", x => x.FullPath);
                });

            migrationBuilder.CreateTable(
                name: "ToScanQueue",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullPathHash = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToScanQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    FullPathHash = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    LastModified = table.Column<DateTime>(nullable: false),
                    LastChecked = table.Column<DateTime>(nullable: false),
                    FolderSize = table.Column<long>(nullable: false),
                    CategoryName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.FullPathHash);
                    table.ForeignKey(
                        name: "FK_Folders_Categories_CategoryName",
                        column: x => x.CategoryName,
                        principalTable: "Categories",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FullPathHash = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FolderHash = table.Column<string>(nullable: true),
                    CategoryName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FullPathHash);
                    table.ForeignKey(
                        name: "FK_Files_Categories_CategoryName",
                        column: x => x.CategoryName,
                        principalTable: "Categories",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Folders_FolderHash",
                        column: x => x.FolderHash,
                        principalTable: "Folders",
                        principalColumn: "FullPathHash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_CategoryName",
                table: "Files",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderHash",
                table: "Files",
                column: "FolderHash");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CategoryName",
                table: "Folders",
                column: "CategoryName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drives");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "FolderExclusions");

            migrationBuilder.DropTable(
                name: "ToScanQueue");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
