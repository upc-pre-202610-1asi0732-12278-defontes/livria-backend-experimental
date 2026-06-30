-- =============================================================================
-- Livria Backend Experimental — datos mock para desarrollo local
-- =============================================================================
-- Prerequisitos:
--   1. MySQL en Docker (livria-mysql-experimental) con livriadb_experimental
--   2. Migraciones aplicadas (dotnet ef database update)
--   3. dotnet run al menos una vez (admin id=1, usuario borrado id=2)
--
-- Ejecutar:
--   .\scripts\seed-mock-data.ps1
--   — o —
--   docker exec -i livria-mysql-experimental mysql -uroot -plivria_exp_root livriadb_experimental < scripts/seed-mock-data.sql
--
-- Usuarios mock (contraseña para todos: 0000):
--   mock_alice  (id 3) — communityplan, dueña de la comunidad
--   mock_bob    (id 4) — freeplan
--   mock_carol  (id 5) — communityplan
-- =============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ---------------------------------------------------------------------------
-- Limpieza (idempotente — solo rango mock)
-- ---------------------------------------------------------------------------
DELETE FROM `comments`       WHERE `Id` BETWEEN 401 AND 499;
DELETE FROM `post_reactions` WHERE `Id` BETWEEN 701 AND 799;
DELETE FROM `posts`          WHERE `Id` BETWEEN 301 AND 399;
DELETE FROM `user_communities` WHERE `CommunityId` = 201;
DELETE FROM `notifications`  WHERE `Id` BETWEEN 601 AND 699;
DELETE FROM `reviews`        WHERE `Id` BETWEEN 501 AND 599;
DELETE FROM `cart_items`     WHERE `Id` BETWEEN 801 AND 899;
DELETE FROM `user_favorite_books` WHERE `UserClientId` IN (3, 4, 5);
DELETE FROM `user_exclusion_books` WHERE `UserClient1Id` IN (3, 4, 5);
DELETE FROM `Identities`     WHERE `UserId` IN (3, 4, 5);
DELETE FROM `userclients`    WHERE `Id` IN (3, 4, 5);
DELETE FROM `users`          WHERE `Id` IN (3, 4, 5);
DELETE FROM `communities`    WHERE `Id` = 201;
DELETE FROM `books`          WHERE `Id` BETWEEN 101 AND 199;

-- Hash BCrypt de "0000" (mismo que admin_default)
SET @pwd_hash = '$2a$11$Q8erpT3.NKGEm5.oT8lAs.bk1fN.gsJYplHWMRjHZgynh/ZVOPcIm';

-- ---------------------------------------------------------------------------
-- Usuarios y clientes
-- ---------------------------------------------------------------------------
INSERT INTO `users` (`Id`, `Display`, `Username`, `Email`) VALUES
  (3, 'Alicia Lectura',  'mock_alice', 'alice.mock@livria.dev'),
  (4, 'Bob Libros',      'mock_bob',   'bob.mock@livria.dev'),
  (5, 'Carol Páginas',   'mock_carol', 'carol.mock@livria.dev');

INSERT INTO `userclients` (`Id`, `Icon`, `Phrase`, `Subscription`, `HasPayed`, `PlanChangeDate`) VALUES
  (3, 'https://i.pravatar.cc/150?u=mock_alice', 'Siempre con un libro en la mochila.', 'communityplan', 1, '2026-05-01 00:00:00.000000'),
  (4, 'https://i.pravatar.cc/150?u=mock_bob',   'Leyendo ciencia ficción.',            'freeplan',      0, NULL),
  (5, 'https://i.pravatar.cc/150?u=mock_carol', 'Club de lectura los martes.',         'communityplan', 1, '2026-05-15 00:00:00.000000');

INSERT INTO `Identities` (`Id`, `UserId`, `UserName`, `hashed_password`) VALUES
  (2, 3, 'mock_alice', @pwd_hash),
  (3, 4, 'mock_bob',   @pwd_hash),
  (4, 5, 'mock_carol', @pwd_hash);

-- ---------------------------------------------------------------------------
-- Libros
-- ---------------------------------------------------------------------------
INSERT INTO `books` (`Id`, `Title`, `Description`, `Author`, `SalePrice`, `PurchasePrice`, `Stock`, `Cover`, `Genre`, `Language`, `IsActive`) VALUES
  (101, 'El nombre del viento',
   'Kvothe, un legendario héroe, cuenta su historia desde la infancia hasta la juventud en la Universidad.',
   'Patrick Rothfuss', 24.90, 12.00, 45,
   'https://picsum.photos/seed/livria-book101/400/600', 'fiction', 'español', 1),
  (102, 'Dune',
   'En el desértico planeta Arrakis, la familia Atreides debe sobrevivir a las intrigas del Imperio.',
   'Frank Herbert', 19.50, 9.80, 30,
   'https://picsum.photos/seed/livria-book102/400/600', 'fiction', 'inglés', 1),
  (103, 'Sapiens',
   'Breve historia de la humanidad: de la revolución cognitiva a la era de la inteligencia artificial.',
   'Yuval Noah Harari', 22.00, 11.00, 60,
   'https://picsum.photos/seed/livria-book103/400/600', 'non_fiction', 'español', 1),
  (104, 'Cien años de soledad',
   'La saga de la familia Buendía en el mítico pueblo de Macondo.',
   'Gabriel García Márquez', 18.75, 9.00, 25,
   'https://picsum.photos/seed/livria-book104/400/600', 'literature', 'español', 1);

INSERT INTO `user_favorite_books` (`UserClientId`, `FavoriteBooksId`) VALUES
  (3, 101), (3, 104),
  (5, 102);

-- ---------------------------------------------------------------------------
-- Comunidad, membresías, posts, comentarios (con y sin img)
-- ---------------------------------------------------------------------------
INSERT INTO `communities` (`Id`, `Name`, `Description`, `OwnerId`, `Type`, `Image`, `Banner`) VALUES
  (201, 'Lectores de Fantasía',
   'Comunidad para compartir recomendaciones de fantasía épica, urbana y distópica.',
   3, 'fiction',
   'https://picsum.photos/seed/livria-comm201/200/200',
   'https://picsum.photos/seed/livria-comm201-banner/1200/400');

INSERT INTO `user_communities` (`UserClientId`, `CommunityId`, `JoinedDate`) VALUES
  (3, 201, '2026-05-01 12:00:00.000000'),
  (4, 201, '2026-05-10 09:30:00.000000'),
  (5, 201, '2026-05-12 18:45:00.000000');

INSERT INTO `posts` (`Id`, `CommunityId`, `UserId`, `Username`, `Content`, `Img`, `CreatedAt`) VALUES
  (301, 201, 3, 'mock_alice',
   '¿Por dónde empezar con fantasía épica? Recomiendo El nombre del viento y Mistborn. ¿Qué opinan?',
   'https://picsum.photos/seed/livria-post301/800/450',
   '2026-06-01 10:00:00.000000'),
  (302, 201, 4, 'mock_bob',
   'Acabo de terminar Dune. La política interplanetaria me voló la cabeza. Sin spoilers, ¿vale la pena leer el resto de la saga?',
   '',
   '2026-06-02 14:20:00.000000'),
  (303, 201, 5, 'mock_carol',
   'Organizamos lectura conjunta de Cien años de soledad para julio. ¿Alguien se apunta?',
   'https://picsum.photos/seed/livria-post303/800/450',
   '2026-06-03 20:15:00.000000');

INSERT INTO `comments` (`Id`, `PostId`, `UserId`, `Username`, `Content`, `Img`, `CreatedAt`) VALUES
  (401, 301, 4, 'mock_bob',
   'Yo empecé por Mistborn y fue un acierto total.',
   '',
   '2026-06-01 11:05:00.000000'),
  (402, 301, 5, 'mock_carol',
   'Mira esta edición ilustrada que encontré:',
   'https://picsum.photos/seed/livria-comment402/600/400',
   '2026-06-01 12:30:00.000000'),
  (403, 302, 3, 'mock_alice',
   'Sí, los primeros tres libros forman una trilogía coherente. El cuarto cambia de tono.',
   '',
   '2026-06-02 16:00:00.000000'),
  (404, 303, 4, 'mock_bob',
   'Me apunto. Comparto mi marcador favorito del libro:',
   'https://picsum.photos/seed/livria-comment404/600/400',
   '2026-06-04 08:10:00.000000'),
  (405, 303, 3, 'mock_alice',
   'Genial, creo un recordatorio en el grupo.',
   '',
   '2026-06-04 09:00:00.000000');

INSERT INTO `post_reactions` (`Id`, `UserId`, `PostId`, `Type`, `UpdatedAt`) VALUES
  (701, 4, 301, 1, '2026-06-01 10:30:00.000000'),
  (702, 5, 301, 1, '2026-06-01 11:00:00.000000'),
  (703, 3, 302, 1, '2026-06-02 15:00:00.000000');

-- ---------------------------------------------------------------------------
-- Reseñas, carrito, notificaciones
-- ---------------------------------------------------------------------------
INSERT INTO `reviews` (`Id`, `Username`, `Content`, `Stars`, `BookId`, `UserClientId`) VALUES
  (501, 'mock_alice', 'Prosa impecable y worldbuilding increíble. Un clásico moderno.', 5, 101, 3),
  (502, 'mock_bob',   'Denso al principio, pero engancha muchísimo después del capítulo 5.', 4, 102, 4);

INSERT INTO `cart_items` (`Id`, `BookId`, `Quantity`, `UserClientId`) VALUES
  (801, 103, 1, 3),
  (802, 104, 2, 5);

INSERT INTO `notifications` (`Id`, `UserClientId`, `CreatedAt`, `Type`, `Title`, `Content`, `IsRead`, `IsHidden`) VALUES
  (601, 3, '2026-06-01 10:05:00.000000', 'Like',    'Nuevo like',     'mock_bob le dio like a tu publicación.', 0, 0),
  (602, 4, '2026-06-03 20:20:00.000000', 'Welcome', 'Bienvenido',     'Te uniste a Lectores de Fantasía.',      1, 0);

-- ---------------------------------------------------------------------------
-- Auto-increment (evita colisiones en inserts futuros)
-- ---------------------------------------------------------------------------
ALTER TABLE `users`          AUTO_INCREMENT = 6;
ALTER TABLE `Identities`     AUTO_INCREMENT = 5;
ALTER TABLE `books`          AUTO_INCREMENT = 105;
ALTER TABLE `communities`    AUTO_INCREMENT = 202;
ALTER TABLE `posts`          AUTO_INCREMENT = 304;
ALTER TABLE `comments`       AUTO_INCREMENT = 406;
ALTER TABLE `post_reactions` AUTO_INCREMENT = 704;
ALTER TABLE `reviews`        AUTO_INCREMENT = 503;
ALTER TABLE `cart_items`     AUTO_INCREMENT = 803;
ALTER TABLE `notifications`  AUTO_INCREMENT = 603;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Seed mock completado.' AS status;
SELECT `Id`, `Username`, `Email` FROM `users` WHERE `Id` IN (3, 4, 5);
SELECT `Id`, `Title`, `Author`, `SalePrice` FROM `books` WHERE `Id` BETWEEN 101 AND 104;
SELECT `Id`, `Name`, `Type`, `OwnerId` FROM `communities` WHERE `Id` = 201;
SELECT `Id`, `PostId`, LEFT(`Content`, 40) AS content_preview,
       CASE WHEN `Img` = '' THEN '(sin img)' ELSE '(con img)' END AS img_flag
FROM `comments` WHERE `Id` BETWEEN 401 AND 405;
