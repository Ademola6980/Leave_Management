using Microsoft.EntityFrameworkCore.Migrations;

namespace Leave_Management.Data.Migrations
{
    public partial class AddedNewFieldToLeaveRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CancelRequest",
                table: "LeaveRequestVM",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CancelRequest",
                table: "LeaveRequests",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelRequest",
                table: "LeaveRequestVM");

            migrationBuilder.DropColumn(
                name: "CancelRequest",
                table: "LeaveRequests");
        }
    }
}
