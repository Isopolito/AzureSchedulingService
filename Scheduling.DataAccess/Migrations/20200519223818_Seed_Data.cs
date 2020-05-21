using Microsoft.EntityFrameworkCore.Migrations;

namespace Scheduling.DataAccess.Migrations
{
    public partial class Seed_Data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scheduling",
                table: "RepeatEndStrategy",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Used" },
                    { 1, "Never" },
                    { 3, "After Occurrence Number" },
                    { 2, "On End Date" }
                });

            migrationBuilder.InsertData(
                schema: "scheduling",
                table: "RepeatInterval",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Not Used" },
                    { 1, "Never" },
                    { 2, "Daily" },
                    { 3, "Weekly" },
                    { 5, "Monthly" },
                    { 4, "Bi-Monthly" },
                    { 6, "Quarterly" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatEndStrategy",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatEndStrategy",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatEndStrategy",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatEndStrategy",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "scheduling",
                table: "RepeatInterval",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
