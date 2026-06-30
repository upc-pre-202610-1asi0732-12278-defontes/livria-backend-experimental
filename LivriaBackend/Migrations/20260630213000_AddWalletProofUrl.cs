using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivriaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletProofUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @proof_url := (
                    SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'wallet_transactions'
                      AND COLUMN_NAME = 'ProofUrl'
                );
                SET @sql := IF(
                    @proof_url = 0,
                    'ALTER TABLE `wallet_transactions` ADD `ProofUrl` varchar(500) NOT NULL DEFAULT ''''',
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
                name: "ProofUrl",
                table: "wallet_transactions");
        }
    }
}
