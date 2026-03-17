using Microsoft.EntityFrameworkCore.Migrations;

public partial class RenameImagerUrlToImageUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ImagerUrl",
            table: "Events",
            newName: "ImageUrl");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ImageUrl",
            table: "Events",
            newName: "ImagerUrl");
    }
}