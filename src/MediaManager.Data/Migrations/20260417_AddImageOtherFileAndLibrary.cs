using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageOtherFileAndLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 添加 ImageFile 列 (Width 和 Height 已在初始迁移中由 VideoFile 创建)
            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "MediaFiles",
                type: "TEXT",
                nullable: true);

            // 添加 OtherFile 列
            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "MediaFiles",
                type: "TEXT",
                nullable: true);

            // 添加 LibraryId 外键
            migrationBuilder.AddColumn<int>(
                name: "LibraryId",
                table: "MediaFiles",
                type: "INTEGER",
                nullable: true);

            // 创建 Libraries 表
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ScanPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Recursive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeAudio = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeVideo = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeImages = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastScannedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FileCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSize = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            // 添加索引
            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_LibraryId",
                table: "MediaFiles",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_ScanPath",
                table: "Libraries",
                column: "ScanPath",
                unique: true);

            // 添加外键约束
            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Libraries_LibraryId",
                table: "MediaFiles",
                column: "LibraryId",
                principalTable: "Libraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Libraries_LibraryId",
                table: "MediaFiles");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_LibraryId",
                table: "MediaFiles");

            // Width 和 Height 由初始迁移创建，不在此迁移中删除
            migrationBuilder.DropColumn(
                name: "Format",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "LibraryId",
                table: "MediaFiles");
        }
    }
}
