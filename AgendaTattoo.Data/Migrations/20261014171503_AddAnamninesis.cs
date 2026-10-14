using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaTattoo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnamninesis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnamnesisForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasAllergies = table.Column<bool>(type: "boolean", nullable: false),
                    AllergiesDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasSkinConditions = table.Column<bool>(type: "boolean", nullable: false),
                    SkinConditionsDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasDiabetes = table.Column<bool>(type: "boolean", nullable: false),
                    HasBloodClottingDisorder = table.Column<bool>(type: "boolean", nullable: false),
                    HasHeartCondition = table.Column<bool>(type: "boolean", nullable: false),
                    HasEpilepsy = table.Column<bool>(type: "boolean", nullable: false),
                    IsPregnantOrBreastfeeding = table.Column<bool>(type: "boolean", nullable: false),
                    UsesBloodThinners = table.Column<bool>(type: "boolean", nullable: false),
                    UsesIsotretinoin = table.Column<bool>(type: "boolean", nullable: false),
                    HasKeloidHistory = table.Column<bool>(type: "boolean", nullable: false),
                    ConsumedAlcoholOrDrugsLast24h = table.Column<bool>(type: "boolean", nullable: false),
                    HadPreviousTattooReaction = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousTattooReactionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CurrentMedications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConsentText = table.Column<string>(type: "text", nullable: false),
                    ConsentAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureFullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SubmittedFromIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnamnesisForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnamnesisForms_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnamnesisForms_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnamnesisForms_Studios_StudioId",
                        column: x => x.StudioId,
                        principalTable: "Studios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnamnesisForms_AppointmentId",
                table: "AnamnesisForms",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnamnesisForms_ClientId",
                table: "AnamnesisForms",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AnamnesisForms_StudioId_ClientId",
                table: "AnamnesisForms",
                columns: new[] { "StudioId", "ClientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnamnesisForms");
        }
    }
}
