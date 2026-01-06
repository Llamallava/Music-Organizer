using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Music_Organizer.Migrations
{
    /// <inheritdoc />
    public partial class AddInterludeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
        name: "IsInterlude",
        table: "TrackReviews",
        type: "INTEGER",
        nullable: false,
        defaultValue: false
    );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
        name: "IsInterlude",
        table: "TrackReviews"
    );
        }
    }
}
