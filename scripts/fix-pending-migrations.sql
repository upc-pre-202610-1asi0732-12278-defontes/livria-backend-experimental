-- Fix manual si dotnet ef database update no aplica las migraciones pendientes.
-- Ejecutar contra livriadb_experimental (puerto 3307 en Docker local).
-- Uso: docker exec -i livria-mysql-experimental mysql -u root -plivria_exp_root livriadb_experimental < scripts/fix-pending-migrations.sql

USE livriadb_experimental;

-- 1) wallet_transactions.ProofUrl (AddWalletProofUrl)
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

-- 2) user_read_books (AddUserReadBooks)
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

-- 3) Registrar migraciones en historial EF (solo si faltan)
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
  ('20260630213000_AddWalletProofUrl', '8.0.26'),
  ('20260630220000_AddUserReadBooks', '8.0.26');

SELECT MigrationId FROM `__EFMigrationsHistory` ORDER BY MigrationId;
