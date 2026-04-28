USE LibraryDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Loans', N'U') IS NULL
   OR OBJECT_ID(N'dbo.BookCopies', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Books', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Readers', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Authors', N'U') IS NULL
BEGIN
    THROW 50000, N'Сначала выполните скрипт Database/LibraryDB.sql для создания структуры базы данных.', 1;
END
GO

BEGIN TRANSACTION;

DECLARE @Authors TABLE
(
    FullName NVARCHAR(100) NOT NULL PRIMARY KEY
);

INSERT INTO @Authors (FullName)
VALUES
    (N'Михаил Булгаков'),
    (N'Александр Пушкин'),
    (N'Федор Достоевский'),
    (N'Лев Толстой'),
    (N'Антон Чехов'),
    (N'Рэй Брэдбери'),
    (N'Джордж Оруэлл'),
    (N'Артур Конан Дойл'),
    (N'Агата Кристи'),
    (N'Жюль Верн');

DECLARE @Readers TABLE
(
    TicketNumber NVARCHAR(30) NOT NULL PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL
);

INSERT INTO @Readers (TicketNumber, FullName, Phone, Address)
VALUES
    (N'RB-1001', N'Иванов Алексей Сергеевич', N'+7 (927) 410-11-01', N'г. Саратов, ул. Московская, д. 12, кв. 8'),
    (N'RB-1002', N'Петрова Мария Викторовна', N'+7 (927) 410-11-02', N'г. Саратов, ул. Чапаева, д. 41, кв. 15'),
    (N'RB-1003', N'Сидоров Илья Андреевич', N'+7 (927) 410-11-03', N'г. Саратов, ул. Рахова, д. 7, кв. 21'),
    (N'RB-1004', N'Кузнецова Елена Павловна', N'+7 (927) 410-11-04', N'г. Энгельс, ул. Полиграфическая, д. 19'),
    (N'RB-1005', N'Орлов Дмитрий Максимович', N'+7 (927) 410-11-05', N'г. Саратов, ул. Астраханская, д. 54, кв. 9'),
    (N'RB-1006', N'Соколова Анна Романовна', N'+7 (927) 410-11-06', N'г. Саратов, ул. Сакко и Ванцетти, д. 27'),
    (N'RB-1007', N'Морозов Кирилл Олегович', N'+7 (927) 410-11-07', N'г. Саратов, пр-т 50 лет Октября, д. 101'),
    (N'RB-1008', N'Федорова Наталья Игоревна', N'+7 (927) 410-11-08', N'г. Саратов, ул. Вольская, д. 33, кв. 4'),
    (N'RB-1009', N'Волков Артем Денисович', N'+7 (927) 410-11-09', N'г. Саратов, ул. Радищева, д. 63, кв. 11'),
    (N'RB-1010', N'Николаева Оксана Сергеевна', N'+7 (927) 410-11-10', N'г. Энгельс, ул. Тельмана, д. 48'),
    (N'RB-1011', N'Громов Павел Евгеньевич', N'+7 (927) 410-11-11', N'г. Саратов, ул. Университетская, д. 72'),
    (N'RB-1012', N'Егорова Софья Алексеевна', N'+7 (927) 410-11-12', N'г. Саратов, ул. Большая Казачья, д. 89');

DECLARE @Books TABLE
(
    Title NVARCHAR(150) NOT NULL,
    AuthorFullName NVARCHAR(100) NOT NULL,
    Publisher NVARCHAR(100) NULL,
    PublishYear INT NULL,
    Genre NVARCHAR(50) NULL,
    ISBN NVARCHAR(30) NULL,
    PRIMARY KEY (Title, AuthorFullName)
);

INSERT INTO @Books (Title, AuthorFullName, Publisher, PublishYear, Genre, ISBN)
VALUES
    (N'Мастер и Маргарита', N'Михаил Булгаков', N'Азбука', 1967, N'Роман', N'978-5-389-07435-4'),
    (N'Евгений Онегин', N'Александр Пушкин', N'Эксмо', 1833, N'Поэма', N'978-5-04-089724-3'),
    (N'Преступление и наказание', N'Федор Достоевский', N'АСТ', 1866, N'Роман', N'978-5-17-090630-3'),
    (N'Война и мир', N'Лев Толстой', N'Эксмо', 1869, N'Роман-эпопея', N'978-5-699-12014-9'),
    (N'Вишневый сад', N'Антон Чехов', N'АСТ', 1904, N'Пьеса', N'978-5-17-089420-4'),
    (N'451 градус по Фаренгейту', N'Рэй Брэдбери', N'Эксмо', 1953, N'Антиутопия', N'978-5-699-78222-4'),
    (N'1984', N'Джордж Оруэлл', N'АСТ', 1949, N'Антиутопия', N'978-5-17-148195-3'),
    (N'Приключения Шерлока Холмса', N'Артур Конан Дойл', N'Махаон', 1892, N'Детектив', N'978-5-389-09185-6'),
    (N'Убийство в Восточном экспрессе', N'Агата Кристи', N'Эксмо', 1934, N'Детектив', N'978-5-699-94514-7'),
    (N'Двадцать тысяч лье под водой', N'Жюль Верн', N'Азбука', 1870, N'Фантастика', N'978-5-389-11524-8'),
    (N'Собачье сердце', N'Михаил Булгаков', N'АСТ', 1925, N'Повесть', N'978-5-17-080120-2'),
    (N'Анна Каренина', N'Лев Толстой', N'Эксмо', 1877, N'Роман', N'978-5-04-105668-7');

DECLARE @Copies TABLE
(
    InventoryNumber NVARCHAR(30) NOT NULL PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    AuthorFullName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100) NOT NULL,
    Status NVARCHAR(30) NOT NULL
);

INSERT INTO @Copies (InventoryNumber, Title, AuthorFullName, Location, Status)
VALUES
    (N'INV-0001', N'Мастер и Маргарита', N'Михаил Булгаков', N'Абонемент, стеллаж A1', N'доступна'),
    (N'INV-0002', N'Мастер и Маргарита', N'Михаил Булгаков', N'Абонемент, стеллаж A1', N'выдана'),
    (N'INV-0003', N'Мастер и Маргарита', N'Михаил Булгаков', N'Резервный фонд, полка B3', N'потеряна'),
    (N'INV-0004', N'Евгений Онегин', N'Александр Пушкин', N'Читальный зал, секция C1', N'доступна'),
    (N'INV-0005', N'Евгений Онегин', N'Александр Пушкин', N'Абонемент, стеллаж A2', N'доступна'),
    (N'INV-0006', N'Преступление и наказание', N'Федор Достоевский', N'Абонемент, стеллаж A3', N'выдана'),
    (N'INV-0007', N'Преступление и наказание', N'Федор Достоевский', N'Читальный зал, секция C2', N'доступна'),
    (N'INV-0008', N'Война и мир', N'Лев Толстой', N'Абонемент, стеллаж A4', N'выдана'),
    (N'INV-0009', N'Война и мир', N'Лев Толстой', N'Абонемент, стеллаж A4', N'доступна'),
    (N'INV-0010', N'Вишневый сад', N'Антон Чехов', N'Читальный зал, секция D1', N'доступна'),
    (N'INV-0011', N'451 градус по Фаренгейту', N'Рэй Брэдбери', N'Абонемент, стеллаж B1', N'выдана'),
    (N'INV-0012', N'451 градус по Фаренгейту', N'Рэй Брэдбери', N'Абонемент, стеллаж B1', N'доступна'),
    (N'INV-0013', N'1984', N'Джордж Оруэлл', N'Абонемент, стеллаж B2', N'выдана'),
    (N'INV-0014', N'1984', N'Джордж Оруэлл', N'Читальный зал, секция C3', N'доступна'),
    (N'INV-0015', N'Приключения Шерлока Холмса', N'Артур Конан Дойл', N'Абонемент, стеллаж C1', N'доступна'),
    (N'INV-0016', N'Приключения Шерлока Холмса', N'Артур Конан Дойл', N'Абонемент, стеллаж C1', N'доступна'),
    (N'INV-0017', N'Убийство в Восточном экспрессе', N'Агата Кристи', N'Абонемент, стеллаж C2', N'выдана'),
    (N'INV-0018', N'Убийство в Восточном экспрессе', N'Агата Кристи', N'Резервный фонд, полка B4', N'доступна'),
    (N'INV-0019', N'Двадцать тысяч лье под водой', N'Жюль Верн', N'Абонемент, стеллаж D2', N'доступна'),
    (N'INV-0020', N'Двадцать тысяч лье под водой', N'Жюль Верн', N'Абонемент, стеллаж D2', N'потеряна'),
    (N'INV-0021', N'Собачье сердце', N'Михаил Булгаков', N'Абонемент, стеллаж A1', N'доступна'),
    (N'INV-0022', N'Собачье сердце', N'Михаил Булгаков', N'Читальный зал, секция C4', N'выдана'),
    (N'INV-0023', N'Анна Каренина', N'Лев Толстой', N'Абонемент, стеллаж A5', N'доступна'),
    (N'INV-0024', N'Анна Каренина', N'Лев Толстой', N'Абонемент, стеллаж A5', N'доступна');

DECLARE @Loans TABLE
(
    TicketNumber NVARCHAR(30) NOT NULL,
    InventoryNumber NVARCHAR(30) NOT NULL,
    IssueDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    ReturnDate DATE NULL,
    PRIMARY KEY (TicketNumber, InventoryNumber, IssueDate)
);

INSERT INTO @Loans (TicketNumber, InventoryNumber, IssueDate, DueDate, ReturnDate)
VALUES
    (N'RB-1008', N'INV-0001', '2026-02-01', '2026-02-15', '2026-02-13'),
    (N'RB-1009', N'INV-0004', '2026-01-10', '2026-01-24', '2026-01-20'),
    (N'RB-1010', N'INV-0007', '2026-03-01', '2026-03-15', '2026-03-18'),
    (N'RB-1001', N'INV-0002', '2026-04-10', '2026-04-24', NULL),
    (N'RB-1002', N'INV-0006', '2026-04-15', '2026-05-05', NULL),
    (N'RB-1003', N'INV-0008', '2026-03-20', '2026-04-03', NULL),
    (N'RB-1004', N'INV-0011', '2026-04-18', '2026-05-08', NULL),
    (N'RB-1005', N'INV-0013', '2026-04-25', '2026-05-15', NULL),
    (N'RB-1006', N'INV-0017', '2026-04-12', '2026-04-26', NULL),
    (N'RB-1007', N'INV-0022', '2026-04-22', '2026-05-12', NULL);

INSERT INTO dbo.Authors (FullName)
SELECT src.FullName
FROM @Authors src
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Authors target
    WHERE target.FullName = src.FullName
);

UPDATE target
SET target.FullName = src.FullName,
    target.Phone = src.Phone,
    target.Address = src.Address
FROM dbo.Readers target
INNER JOIN @Readers src ON src.TicketNumber = target.TicketNumber;

INSERT INTO dbo.Readers (FullName, Phone, Address, TicketNumber)
SELECT src.FullName, src.Phone, src.Address, src.TicketNumber
FROM @Readers src
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Readers target
    WHERE target.TicketNumber = src.TicketNumber
);

UPDATE target
SET target.Publisher = src.Publisher,
    target.PublishYear = src.PublishYear,
    target.Genre = src.Genre,
    target.ISBN = src.ISBN
FROM dbo.Books target
INNER JOIN dbo.Authors authorTarget ON authorTarget.AuthorId = target.AuthorId
INNER JOIN @Books src
    ON src.Title = target.Title
   AND src.AuthorFullName = authorTarget.FullName;

INSERT INTO dbo.Books (Title, AuthorId, Publisher, PublishYear, Genre, ISBN)
SELECT src.Title,
       author.AuthorId,
       src.Publisher,
       src.PublishYear,
       src.Genre,
       src.ISBN
FROM @Books src
INNER JOIN dbo.Authors author ON author.FullName = src.AuthorFullName
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Books target
    WHERE target.Title = src.Title
      AND target.AuthorId = author.AuthorId
);

DELETE loan
FROM dbo.Loans loan
INNER JOIN dbo.BookCopies copy ON copy.CopyId = loan.CopyId
INNER JOIN @Copies src ON src.InventoryNumber = copy.InventoryNumber;

UPDATE target
SET target.BookId = bookTarget.BookId,
    target.Location = src.Location,
    target.Status = src.Status
FROM dbo.BookCopies target
INNER JOIN @Copies src ON src.InventoryNumber = target.InventoryNumber
INNER JOIN dbo.Authors author ON author.FullName = src.AuthorFullName
INNER JOIN dbo.Books bookTarget
    ON bookTarget.Title = src.Title
   AND bookTarget.AuthorId = author.AuthorId;

INSERT INTO dbo.BookCopies (BookId, InventoryNumber, Location, Status)
SELECT bookTarget.BookId,
       src.InventoryNumber,
       src.Location,
       src.Status
FROM @Copies src
INNER JOIN dbo.Authors author ON author.FullName = src.AuthorFullName
INNER JOIN dbo.Books bookTarget
    ON bookTarget.Title = src.Title
   AND bookTarget.AuthorId = author.AuthorId
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.BookCopies target
    WHERE target.InventoryNumber = src.InventoryNumber
);

INSERT INTO dbo.Loans (ReaderId, CopyId, IssueDate, DueDate, ReturnDate)
SELECT readerTarget.ReaderId,
       copyTarget.CopyId,
       src.IssueDate,
       src.DueDate,
       src.ReturnDate
FROM @Loans src
INNER JOIN dbo.Readers readerTarget ON readerTarget.TicketNumber = src.TicketNumber
INNER JOIN dbo.BookCopies copyTarget ON copyTarget.InventoryNumber = src.InventoryNumber;

COMMIT TRANSACTION;
GO
