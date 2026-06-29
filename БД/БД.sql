-- Создание базы данных
CREATE DATABASE [MunicipalOlympiads];
GO

USE [MunicipalOlympiads];
GO

-- Таблица: Учитель (Фамилия, имя, отчество ПОЛНОСТЬЮ)
CREATE TABLE [dbo].[Учитель](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ФИО] [nvarchar](150) NOT NULL,  -- Полностью фамилия, имя, отчество
	[Телефон] [nvarchar](11) NULL,
	[Email] [nvarchar](50) NULL,
	CONSTRAINT [PK_Учитель] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Таблица: Класс (справочник классов)
CREATE TABLE [dbo].[Класс](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Название] [nvarchar](10) NOT NULL,  -- например, "9А", "10Б", "11В"
	CONSTRAINT [PK_Класс] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Таблица: Школьник
CREATE TABLE [dbo].[Школьник](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Фамилия] [nvarchar](50) NOT NULL,
	[Имя] [nvarchar](50) NOT NULL,
	[Отчество] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NOT NULL,  -- E-mail участника
	[Телефон] [nvarchar](20) NOT NULL,  -- Телефон участника
	[Класс_ID] [int] NOT NULL,  -- Класс, в котором учится школьник
	CONSTRAINT [PK_Школьник] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Таблица: Олимпиада
CREATE TABLE [dbo].[Олимпиада](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Название] [nvarchar](100) NOT NULL,
	[Предмет] [nvarchar](50) NOT NULL,
	[Дата_проведения] [date] NOT NULL,
	CONSTRAINT [PK_Олимпиада] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Таблица: Участие_в_олимпиаде
-- Здесь фиксируется, за какой класс ученик выполнял задания и какой учитель его сопровождал
CREATE TABLE [dbo].[Участие_в_олимпиаде](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Школьник_ID] [int] NOT NULL,
	[Олимпиада_ID] [int] NOT NULL,
	[Класс_за_который_выполнял_ID] [int] NOT NULL,  -- Класс, за который выполнял олимпиадные задания
	[Учитель_ID] [int] NOT NULL,  -- Фамилия, имя, отчество учителя ПОЛНОСТЬЮ
	[Количество_баллов] [int] NULL,
	[Результат_участия] [nvarchar](20) NULL,
	CONSTRAINT [PK_Участие_в_олимпиаде] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

-- Представление: Сведения_об_участниках_олимпиад
-- Выводит всю необходимую информацию по заданию
CREATE VIEW [dbo].[Сведения_об_участниках_олимпиад]
AS
SELECT 
    CONCAT(ш.Фамилия, ' ', ш.Имя, ISNULL(' ' + ш.Отчество, '')) AS [ФИО участника],
    кл1.Название AS [Класс_в_котором_учится],
    кл2.Название AS [Класс_за_который_выполнял],
    ш.Email AS [E-mail участника],
    ш.Телефон AS [Телефон участника],
    у.ФИО AS [ФИО учителя ПОЛНОСТЬЮ],
    о.Название AS [Олимпиада],
    о.Предмет,
    о.Дата_проведения,
    уо.Количество_баллов,
    уо.Результат_участия
FROM [dbo].[Участие_в_олимпиаде] уо
INNER JOIN [dbo].[Школьник] ш ON уо.Школьник_ID = ш.ID
INNER JOIN [dbo].[Класс] кл1 ON ш.Класс_ID = кл1.ID
INNER JOIN [dbo].[Класс] кл2 ON уо.Класс_за_который_выполнял_ID = кл2.ID
INNER JOIN [dbo].[Учитель] у ON уо.Учитель_ID = у.ID
INNER JOIN [dbo].[Олимпиада] о ON уо.Олимпиада_ID = о.ID;
GO

-- Представление: Сведения_об_учителях_и_их_учениках
CREATE VIEW [dbo].[Сведения_об_учителях_и_их_учениках]
AS
SELECT 
    у.ФИО AS [ФИО учителя ПОЛНОСТЬЮ],
    у.Телефон AS [Телефон учителя],
    у.Email AS [Email учителя],
    COUNT(DISTINCT уо.Школьник_ID) AS [Количество подготовленных учеников],
    COUNT(уо.ID) AS [Количество участий в олимпиадах]
FROM [dbo].[Учитель] у
LEFT JOIN [dbo].[Участие_в_олимпиаде] уо ON у.ID = уо.Учитель_ID
GROUP BY у.ID, у.ФИО, у.Телефон, у.Email;
GO

-- Внешние ключи
ALTER TABLE [dbo].[Школьник] ADD CONSTRAINT [FK_Школьник_Класс] FOREIGN KEY([Класс_ID]) REFERENCES [dbo].[Класс] ([ID]);
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [FK_Участие_в_олимпиаде_Школьник] FOREIGN KEY([Школьник_ID]) REFERENCES [dbo].[Школьник] ([ID]);
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [FK_Участие_в_олимпиаде_Олимпиада] FOREIGN KEY([Олимпиада_ID]) REFERENCES [dbo].[Олимпиада] ([ID]);
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [FK_Участие_в_олимпиаде_Класс] FOREIGN KEY([Класс_за_который_выполнял_ID]) REFERENCES [dbo].[Класс] ([ID]);
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [FK_Участие_в_олимпиаде_Учитель] FOREIGN KEY([Учитель_ID]) REFERENCES [dbo].[Учитель] ([ID]);
GO

-- CHECK-ограничения
ALTER TABLE [dbo].[Школьник] ADD CONSTRAINT [CHK_Email_Format] CHECK (([Email] LIKE '%_@__%.__%'));
GO

ALTER TABLE [dbo].[Школьник] ADD CONSTRAINT [CHK_Phone_Format] CHECK (([Телефон] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' OR [Телефон] LIKE '+[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'));
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [CHK_Баллы] CHECK (([Количество_баллов] >= 0 AND [Количество_баллов] <= 100));
GO

ALTER TABLE [dbo].[Участие_в_олимпиаде] ADD CONSTRAINT [CHK_Результат] CHECK ((LOWER([Результат_участия]) = N'участник' OR LOWER([Результат_участия]) = N'победитель' OR LOWER([Результат_участия]) = N'призёр' OR [Результат_участия] IS NULL));
GO