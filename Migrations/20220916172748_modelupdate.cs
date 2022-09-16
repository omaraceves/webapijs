using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserDeviceApi.Migrations
{
    /// <inheritdoc />
    public partial class modelupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDevices_UserDeviceCodes_UserDeviceCodeId1",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_UserDeviceCodeId1",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "UserDeviceCodeId",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "UserDeviceCodeId1",
                table: "UserDevices");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceCodes_UserDeviceId",
                table: "UserDeviceCodes",
                column: "UserDeviceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDeviceCodes_UserDevices_UserDeviceId",
                table: "UserDeviceCodes",
                column: "UserDeviceId",
                principalTable: "UserDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDeviceCodes_UserDevices_UserDeviceId",
                table: "UserDeviceCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserDeviceCodes_UserDeviceId",
                table: "UserDeviceCodes");

            migrationBuilder.AddColumn<Guid>(
                name: "UserDeviceCodeId",
                table: "UserDevices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserDeviceCodeId1",
                table: "UserDevices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserDeviceCodeId1",
                table: "UserDevices",
                column: "UserDeviceCodeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDevices_UserDeviceCodes_UserDeviceCodeId1",
                table: "UserDevices",
                column: "UserDeviceCodeId1",
                principalTable: "UserDeviceCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
