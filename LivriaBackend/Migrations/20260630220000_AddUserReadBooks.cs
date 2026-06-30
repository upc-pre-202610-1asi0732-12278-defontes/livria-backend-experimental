using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivriaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserReadBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @tbl := (
                    SELECT COUNT(*) FROM information_schema.TABLES
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'user_read_books'
                );
                SET @sql := IF(
                    @tbl = 0,
                    'CREATE TABLE `user_read_books` (
                        `ReadBooksId` int NOT NULL,
                        `UserClientId` int NOT NULL,
                        PRIMARY KEY (`ReadBooksId`, `UserClientId`),
                        KEY `IX_user_read_books_UserClientId` (`UserClientId`),
                        CONSTRAINT `FK_user_read_books_books_ReadBooksId` FOREIGN KEY (`ReadBooksId`) REFERENCES `books` (`Id`) ON DELETE CASCADE,
                        CONSTRAINT `FK_user_read_books_userclients_UserClientId` FOREIGN KEY (`UserClientId`) REFERENCES `userclients` (`Id`) ON DELETE CASCADE
                    )',
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
            migrationBuilder.DropTable(name: "user_read_books");
        }
    }
}
