using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaManager.Data.Migrations
{
    public partial class FixThumbnailPathColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 将 ThumbnailPath 重命名为 VideoFile_ThumbnailPath
            migrationBuilder.RenameColumn(
                name: "ThumbnailPath",
                table: "MediaFiles",
                newName: "VideoFile_ThumbnailPath");

            // 添加 ImageFile_ThumbnailPath 列
            migrationBuilder.AddColumn<string>(
                name: "ImageFile_ThumbnailPath",
                table: "MediaFiles",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除 ImageFile_ThumbnailPath 列
            migrationBuilder.DropColumn(
                name: "ImageFile_ThumbnailPath",
                table: "MediaFiles");

            // 将 VideoFile_ThumbnailPath 重命名回 ThumbnailPath
            migrationBuilder.RenameColumn(
                name: "VideoFile_ThumbnailPath",
                table: "MediaFiles",
                newName: "ThumbnailPath");
        }
    }
}