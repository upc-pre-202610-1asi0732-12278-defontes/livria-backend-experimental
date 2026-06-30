using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivriaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: MySQL no permite DEFAULT en longtext; la migración pudo quedar a medias.
            migrationBuilder.Sql("""
                SET @has_payed := (
                    SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'userclients'
                      AND COLUMN_NAME = 'HasPayed'
                );
                SET @sql := IF(
                    @has_payed = 0,
                    'ALTER TABLE `userclients` ADD `HasPayed` tinyint(1) NOT NULL DEFAULT 0',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @plan_change := (
                    SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'userclients'
                      AND COLUMN_NAME = 'PlanChangeDate'
                );
                SET @sql := IF(
                    @plan_change = 0,
                    'ALTER TABLE `userclients` ADD `PlanChangeDate` datetime(6) NULL',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @icon_nullable := (
                    SELECT IS_NULLABLE FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'userclients'
                      AND COLUMN_NAME = 'Icon'
                    LIMIT 1
                );
                SET @sql := IF(
                    @icon_nullable = 'NO',
                    'ALTER TABLE `userclients` MODIFY `Icon` longtext NULL',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("""
                SET @comment_img := (
                    SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'comments'
                      AND COLUMN_NAME = 'Img'
                );
                SET @sql := IF(
                    @comment_img = 0,
                    'ALTER TABLE `comments` ADD `Img` longtext NULL',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);

            migrationBuilder.Sql("UPDATE `comments` SET `Img` = '' WHERE `Img` IS NULL;");

            migrationBuilder.Sql("""
                SET @comment_img_nullable := (
                    SELECT IS_NULLABLE FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'comments'
                      AND COLUMN_NAME = 'Img'
                    LIMIT 1
                );
                SET @sql := IF(
                    @comment_img_nullable = 'YES',
                    'ALTER TABLE `comments` MODIFY `Img` longtext NOT NULL',
                    'SELECT 1'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPayed",
                table: "userclients");

            migrationBuilder.DropColumn(
                name: "PlanChangeDate",
                table: "userclients");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "comments");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "userclients",
                type: "longtext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);
        }
    }
}
