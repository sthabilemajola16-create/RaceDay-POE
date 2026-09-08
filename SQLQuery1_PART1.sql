SET NOCOUNT ON;
GO

-- Drop database if it exists
USE master;
GO

IF DB_ID(N'RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

-- Create fresh database
CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- Drop tables if they exist for safe rerun (in correct dependency order)
IF OBJECT_ID('dbo.Result', 'U') IS NOT NULL DROP TABLE dbo.Result;
IF OBJECT_ID('dbo.Enrolment', 'U') IS NOT NULL DROP TABLE dbo.Enrolment;
IF OBJECT_ID('dbo.WeatherSnapshot', 'U') IS NOT NULL DROP TABLE dbo.WeatherSnapshot;
IF OBJECT_ID('dbo.Route', 'U') IS NOT NULL DROP TABLE dbo.Route;
IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category;
IF OBJECT_ID('dbo.RaceEvent', 'U') IS NOT NULL DROP TABLE dbo.RaceEvent;
IF OBJECT_ID('dbo.AppUser', 'U') IS NOT NULL DROP TABLE dbo.AppUser;
IF OBJECT_ID('dbo.Club', 'U') IS NOT NULL DROP TABLE dbo.Club;
GO

-- Create tables
CREATE TABLE dbo.Club
(
    ClubId      INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(150)     NOT NULL,
    Province    NVARCHAR(50)      NOT NULL,
    City        NVARCHAR(100)     NOT NULL,
    FoundedYear INT               NULL,
    CONSTRAINT PK_Club PRIMARY KEY (ClubId),
    CONSTRAINT UQ_Club_Name UNIQUE (Name),
    CONSTRAINT CK_Club_FoundedYear CHECK (FoundedYear IS NULL OR FoundedYear BETWEEN 1800 AND 2100)
);
GO

CREATE TABLE dbo.AppUser
(
    UserId       INT IDENTITY(1,1) NOT NULL,
    ClubId       INT               NULL,
    Email        NVARCHAR(256)     NOT NULL,
    PasswordHash NVARCHAR(256)     NOT NULL,
    FirstName    NVARCHAR(80)      NOT NULL,
    LastName     NVARCHAR(80)      NOT NULL,
    Role         NVARCHAR(20)      NOT NULL,
    Phone        NVARCHAR(20)      NULL,
    DateOfBirth  DATE              NULL,
    CreatedAt    DATETIME2(0)      NOT NULL CONSTRAINT DF_AppUser_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AppUser PRIMARY KEY (UserId),
    CONSTRAINT UQ_AppUser_Email UNIQUE (Email),
    CONSTRAINT FK_AppUser_Club FOREIGN KEY (ClubId) REFERENCES dbo.Club (ClubId),
    CONSTRAINT CK_AppUser_Role CHECK (Role IN (N'Organiser', N'Participant'))
);
GO

CREATE TABLE dbo.RaceEvent
(
    EventId        INT IDENTITY(1,1) NOT NULL,
    OrganiserId    INT               NOT NULL,
    Name           NVARCHAR(200)     NOT NULL,
    Description    NVARCHAR(MAX)     NULL,
    SportType      NVARCHAR(30)      NOT NULL,
    Venue          NVARCHAR(200)     NOT NULL,
    City           NVARCHAR(100)     NOT NULL,
    Province       NVARCHAR(50)      NOT NULL,
    StartDateTime  DATETIME2(0)      NOT NULL,
    EndDateTime    DATETIME2(0)      NOT NULL,
    Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_RaceEvent_Status DEFAULT (N'Published'),
    RouteSummary   NVARCHAR(500)     NULL,
    CreatedAt      DATETIME2(0)      NOT NULL CONSTRAINT DF_RaceEvent_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_RaceEvent PRIMARY KEY (EventId),
    CONSTRAINT FK_RaceEvent_Organiser FOREIGN KEY (OrganiserId) REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_RaceEvent_SportType CHECK (SportType IN (N'Running', N'Walking', N'Cycling')),
    CONSTRAINT CK_RaceEvent_Status CHECK (Status IN (N'Draft', N'Published', N'Closed', N'Completed')),
    CONSTRAINT CK_RaceEvent_Dates CHECK (EndDateTime > StartDateTime)
);
GO

CREATE TABLE dbo.Category
(
    CategoryId      INT IDENTITY(1,1) NOT NULL,
    EventId         INT               NOT NULL,
    Name            NVARCHAR(80)      NOT NULL,
    DistanceKm      DECIMAL(6,2)      NOT NULL,
    EntryFeeZAR     DECIMAL(10,2)     NOT NULL,
    MaxParticipants INT               NULL,
    MinAge          INT               NULL,
    MaxAge          INT               NULL,
    CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Category_RaceEvent FOREIGN KEY (EventId) REFERENCES dbo.RaceEvent (EventId),
    CONSTRAINT CK_Category_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Category_Fee CHECK (EntryFeeZAR >= 0),
    CONSTRAINT CK_Category_MaxParticipants CHECK (MaxParticipants IS NULL OR MaxParticipants > 0),
    CONSTRAINT CK_Category_Age CHECK (
        (MinAge IS NULL OR MinAge >= 0) AND
        (MaxAge IS NULL OR MaxAge >= 0) AND
        (MinAge IS NULL OR MaxAge IS NULL OR MaxAge >= MinAge)
    )
);
GO

CREATE TABLE dbo.Route
(
    RouteId         INT IDENTITY(1,1) NOT NULL,
    EventId         INT               NOT NULL,
    Name            NVARCHAR(120)     NOT NULL,
    DistanceKm      DECIMAL(6,2)      NOT NULL,
    ElevationGainM  INT               NULL,
    MapNotes        NVARCHAR(MAX)     NULL,
    GpxUrl          NVARCHAR(500)     NULL,
    CONSTRAINT PK_Route PRIMARY KEY (RouteId),
    CONSTRAINT FK_Route_RaceEvent FOREIGN KEY (EventId) REFERENCES dbo.RaceEvent (EventId),
    CONSTRAINT CK_Route_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Route_Elevation CHECK (ElevationGainM IS NULL OR ElevationGainM >= 0)
);
GO

CREATE TABLE dbo.WeatherSnapshot
(
    WeatherSnapshotId INT IDENTITY(1,1) NOT NULL,
    EventId           INT               NOT NULL,
    CapturedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_WeatherSnapshot_CapturedAt DEFAULT (SYSUTCDATETIME()),
    TemperatureC      DECIMAL(4,1)      NOT NULL,
    Condition         NVARCHAR(80)      NOT NULL,
    WindKph           DECIMAL(5,1)      NULL,
    HumidityPercent   INT               NULL,
    Source            NVARCHAR(80)      NOT NULL CONSTRAINT DF_WeatherSnapshot_Source DEFAULT (N'OpenWeather'),
    CONSTRAINT PK_WeatherSnapshot PRIMARY KEY (WeatherSnapshotId),
    CONSTRAINT FK_WeatherSnapshot_RaceEvent FOREIGN KEY (EventId) REFERENCES dbo.RaceEvent (EventId),
    CONSTRAINT CK_Weather_Humidity CHECK (HumidityPercent IS NULL OR HumidityPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_Weather_Wind CHECK (WindKph IS NULL OR WindKph >= 0)
);
GO

CREATE TABLE dbo.Enrolment
(
    EnrolmentId    INT IDENTITY(1,1) NOT NULL,
    EventId        INT               NOT NULL,
    CategoryId     INT               NOT NULL,
    ParticipantId  INT               NOT NULL,
    EnrolmentDate  DATETIME2(0)      NOT NULL CONSTRAINT DF_Enrolment_Date DEFAULT (SYSUTCDATETIME()),
    Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Enrolment_Status DEFAULT (N'Confirmed'),
    BibNumber      INT               NULL,
    CONSTRAINT PK_Enrolment PRIMARY KEY (EnrolmentId),
    CONSTRAINT FK_Enrolment_RaceEvent FOREIGN KEY (EventId) REFERENCES dbo.RaceEvent (EventId),
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Category (CategoryId),
    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.AppUser (UserId),
    CONSTRAINT UQ_Enrolment_Event_Participant UNIQUE (EventId, ParticipantId),
    CONSTRAINT UQ_Enrolment_Event_Bib UNIQUE (EventId, BibNumber),
    CONSTRAINT CK_Enrolment_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Withdrawn', N'DNS', N'DNF')),
    CONSTRAINT CK_Enrolment_Bib CHECK (BibNumber IS NULL OR BibNumber > 0)
);
GO

CREATE TABLE dbo.Result
(
    ResultId               INT IDENTITY(1,1) NOT NULL,
    EnrolmentId            INT               NOT NULL,
    FinishTime             TIME(0)           NULL,
    PositionOverall        INT               NULL,
    ResultStatus           NVARCHAR(20)      NOT NULL,
    CapturedByOrganiserId  INT               NOT NULL,
    CapturedAt             DATETIME2(0)      NOT NULL CONSTRAINT DF_Result_CapturedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Result PRIMARY KEY (ResultId),
    CONSTRAINT UQ_Result_Enrolment UNIQUE (EnrolmentId),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolment (EnrolmentId),
    CONSTRAINT FK_Result_Organiser FOREIGN KEY (CapturedByOrganiserId) REFERENCES dbo.AppUser (UserId),
    CONSTRAINT CK_Result_Status CHECK (ResultStatus IN (N'Finished', N'DNF', N'DSQ', N'DNS')),
    CONSTRAINT CK_Result_Position CHECK (PositionOverall IS NULL OR PositionOverall > 0)
);
GO

-- Seed data – realistic South African road-event sample
INSERT INTO dbo.Club (Name, Province, City, FoundedYear)
VALUES
    (N'KwaZulu-Natal Athletics Club', N'KwaZulu-Natal', N'Durban', 1988),
    (N'Cape Town Road Cycling Club', N'Western Cape', N'Cape Town', 1995);
GO

INSERT INTO dbo.AppUser (ClubId, Email, PasswordHash, FirstName, LastName, Role, Phone, DateOfBirth)
VALUES
    (1, N'thabo.organiser@raceday.co.za',  N'HASH$organiser1', N'Thabo',  N'Mokoena',  N'Organiser',   N'+27821230001', '1984-03-12'),
    (2, N'ayesha.organiser@raceday.co.za', N'HASH$organiser2', N'Ayesha', N'Davids',   N'Organiser',   N'+27821230002', '1988-11-04'),
    (1, N'lerato.runner@email.com',        N'HASH$participant1', N'Lerato', N'Nkosi', N'Participant', N'+27834560011', '1996-07-21'),
    (2, N'pieter.cyclist@email.com',       N'HASH$participant2', N'Pieter', N'Botha', N'Participant', N'+27834560012', '1992-01-30');
GO

INSERT INTO dbo.RaceEvent
    (OrganiserId, Name, Description, SportType, Venue, City, Province, StartDateTime, EndDateTime, Status, RouteSummary)
VALUES
    (1,
     N'Soweto Sunrise 10 km',
     N'Community road run through Orlando and Vilakazi Street precinct. Paper entries replaced by RaceDay digital enrolment.',
     N'Running',
     N'Orlando Stadium precinct',
     N'Soweto',
     N'Gauteng',
     '2026-09-20 06:30:00',
     '2026-09-20 11:00:00',
     N'Published',
     N'Out-and-back on closed suburban roads. Water table at 5 km.'),
    (1,
     N'Durban Beachfront Half Marathon',
     N'Point to uShaka coastal 21.1 km. Organisers capture results on RaceDay after the finish chute.',
     N'Running',
     N'North Beach start, Durban',
     N'Durban',
     N'KwaZulu-Natal',
     '2026-10-11 06:00:00',
     '2026-10-11 12:30:00',
     N'Published',
     N'Flat seafront route. Wind can be strong after 08:00.'),
    (2,
     N'Stellenbosch Winelands Cycle Classic',
     N'Sportive ride for the Western Cape cycling community. Categories by distance, not only elite racing.',
     N'Cycling',
     N'Markotter Stadium, Stellenbosch',
     N'Stellenbosch',
     N'Western Cape',
     '2026-11-08 07:00:00',
     '2026-11-08 16:00:00',
     N'Published',
     N'Rolling farm roads. Feed zone at 42 km on the 80 km route.');
GO

INSERT INTO dbo.Category (EventId, Name, DistanceKm, EntryFeeZAR, MaxParticipants, MinAge, MaxAge)
VALUES
    (1, N'5 km Fun Run',          5.00,  80.00,  400, 12, NULL),
    (1, N'10 km Open',           10.00, 150.00,  800, 16, NULL),
    (2, N'10 km Companion Run',  10.00, 180.00,  500, 16, NULL),
    (2, N'21.1 km Half Marathon', 21.10, 280.00, 1500, 18, NULL),
    (3, N'40 km Sportive',       40.00, 350.00,  600, 16, NULL),
    (3, N'80 km Classic',        80.00, 520.00,  400, 18, NULL);
GO

INSERT INTO dbo.Route (EventId, Name, DistanceKm, ElevationGainM, MapNotes, GpxUrl)
VALUES
    (1, N'Soweto 10 km official', 10.00,  95,
     N'Start at stadium gates, turn at Vilakazi Street, return on the same carriageway.',
     N'https://blobs.raceday.example/routes/soweto-10km.gpx'),
    (2, N'Durban half marathon', 21.10,  40,
     N'Seafront promenade to uShaka and back. Keep left of the cones.',
     N'https://blobs.raceday.example/routes/durban-21km.gpx'),
    (3, N'Winelands 80 km', 80.00, 980,
     N'Helshoogte climb is the main challenge. Neutralised start for 2 km.',
     N'https://blobs.raceday.example/routes/stellenbosch-80km.gpx');
GO

INSERT INTO dbo.WeatherSnapshot (EventId, CapturedAt, TemperatureC, Condition, WindKph, HumidityPercent, Source)
VALUES
    (1, '2026-09-19 18:00:00', 14.2, N'Clear',        8.5,  55, N'OpenWeather'),
    (2, '2026-10-10 18:00:00', 19.8, N'Partly cloudy', 22.0, 72, N'OpenWeather'),
    (3, '2026-11-07 18:00:00', 17.1, N'Sunny',        12.4, 48, N'OpenWeather');
GO

INSERT INTO dbo.Enrolment (EventId, CategoryId, ParticipantId, EnrolmentDate, Status, BibNumber)
VALUES
    (1, 2, 3, '2026-08-01 09:15:00', N'Confirmed', 101),
    (2, 4, 3, '2026-08-12 14:40:00', N'Confirmed', 214),
    (3, 6, 4, '2026-08-18 11:05:00', N'Confirmed',  88);
GO

INSERT INTO dbo.Result (EnrolmentId, FinishTime, PositionOverall, ResultStatus, CapturedByOrganiserId, CapturedAt)
VALUES
    (1, '00:48:22', 12, N'Finished', 1, '2026-09-20 10:05:00');
GO

-- Verification query (to confirm data imported)
SELECT 'Club' AS TableName, COUNT(*) AS TotalRows FROM dbo.Club
UNION ALL SELECT 'AppUser', COUNT(*) FROM dbo.AppUser
UNION ALL SELECT 'RaceEvent', COUNT(*) FROM dbo.RaceEvent
UNION ALL SELECT 'Category', COUNT(*) FROM dbo.Category
UNION ALL SELECT 'Route', COUNT(*) FROM dbo.Route
UNION ALL SELECT 'WeatherSnapshot', COUNT(*) FROM dbo.WeatherSnapshot
UNION ALL SELECT 'Enrolment', COUNT(*) FROM dbo.Enrolment
UNION ALL SELECT 'Result', COUNT(*) FROM dbo.Result;
GO
