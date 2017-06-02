using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WizBot.Migrations
{
    public partial class guildprefixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "GuildConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPrefix",
                table: "BotConfig",
                nullable: true,
                defaultValue: ".");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "DefaultPrefix",
                table: "BotConfig");
        }
    }
}