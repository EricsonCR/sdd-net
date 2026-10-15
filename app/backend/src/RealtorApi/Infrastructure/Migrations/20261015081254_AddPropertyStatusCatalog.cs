using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtorApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyStatusCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "property_status",
                columns: table => new
                {
                    Value = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_status", x => x.Value);
                    table.CheckConstraint("CK_property_status_value_not_empty", "\"Value\" <> ''");
                });

            migrationBuilder.InsertData(
                table: "property_status",
                columns: new[] { "Value", "Description" },
                values: new object[,]
                {
                    { "Available", "La propiedad está disponible para renta" },
                    { "Rented", "La propiedad tiene un inquilino activo" },
                    { "Maintenance", "La propiedad está fuera de servicio temporalmente" }
                });

            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    unknown_statuses text;
                BEGIN
                    SELECT string_agg(DISTINCT property."Status", ', ' ORDER BY property."Status")
                    INTO unknown_statuses
                    FROM "Properties" AS property
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM property_status AS status
                        WHERE status."Value" = property."Status");

                    IF unknown_statuses IS NOT NULL THEN
                        RAISE EXCEPTION 'Migration aborted; unknown property statuses: %', unknown_statuses;
                    END IF;
                END
                $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Status",
                table: "Properties",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_property_status_Status",
                table: "Properties",
                column: "Status",
                principalTable: "property_status",
                principalColumn: "Value",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_property_status_Status",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "property_status");

            migrationBuilder.DropIndex(
                name: "IX_Properties_Status",
                table: "Properties");
        }
    }
}
