-- Drop the database if it exists
DROP TABLE IF EXISTS "GenreRelations" CASCADE;
DROP TABLE IF EXISTS "Genres" CASCADE;
DROP TABLE IF EXISTS "Clusters" CASCADE;
DROP TABLE IF EXISTS "Artists" CASCADE;
DROP TABLE IF EXISTS "Fanaticos" CASCADE;
DROP TABLE IF EXISTS "FanaticoGenres" CASCADE;

DROP DATABASE IF EXISTS "MusicTreeDB";
-- Drop the user if exists 
-- DROP USER IF EXISTS musictree_user;

-- Create the database
CREATE DATABASE "MusicTreeDB"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Grant privileges (uncomment if using separate user)
GRANT ALL PRIVILEGES ON DATABASE "MusicTreeDB" TO musictree_user;

-- Connect to the new database
\c "MusicTreeDB";

-- ============================================================================
-- PART 2: EXTENSIONS AND BASIC SETUP
-- ============================================================================

-- Enable UUID extension for generating UUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable PostgreSQL crypto extension (useful for hashing)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- PART 3: SCHEMA CREATION (Based on your Entity Framework models)
-- ============================================================================

-- Create Clusters table
CREATE TABLE "Clusters" (
                            "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                            "Name" VARCHAR(30) NOT NULL,
                            "Description" VARCHAR(300),
                            "IsActive" BOOLEAN NOT NULL DEFAULT true,
                            "TimeStamp" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                            CONSTRAINT "PK_Clusters" PRIMARY KEY ("Id"),
                            CONSTRAINT "UQ_Clusters_Name" UNIQUE ("Name")
);

-- Create Genres table
CREATE TABLE "Genres" (
                          "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                          "Name" VARCHAR(30) NOT NULL,
                          "Description" VARCHAR(1000),
                          "GenreOriginCountry" VARCHAR(100),
                          "ColorR" INTEGER,
                          "ColorG" INTEGER,
                          "ColorB" INTEGER,
                          "IsActive" BOOLEAN NOT NULL DEFAULT true,
                          "TimeStamp" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                          "Key" INTEGER NOT NULL DEFAULT -1,
                          "Volume" INTEGER NOT NULL DEFAULT -20,
                          "CompasMetric" INTEGER NOT NULL DEFAULT 4,
                          "Bpm" INTEGER NOT NULL DEFAULT 120,
                          "BpmLower" INTEGER,
                          "BpmUpper" INTEGER,
                          "AvrgDuration" INTEGER,
                          "GenreCreationYear" INTEGER,
                          "GenreTipicalMode" INTEGER,
                          "IsSubgenre" BOOLEAN NOT NULL DEFAULT false,
                          "ParentGenreId" VARCHAR(128),
                          "ClusterId" VARCHAR(128),
                          CONSTRAINT "PK_Genres" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Genres_ParentGenre" FOREIGN KEY ("ParentGenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT,
                          CONSTRAINT "FK_Genres_Cluster" FOREIGN KEY ("ClusterId") REFERENCES "Clusters" ("Id") ON DELETE SET NULL,
                          CONSTRAINT "CK_Genres_ColorR" CHECK ("ColorR" IS NULL OR ("ColorR" >= 0 AND "ColorR" <= 255)),
                          CONSTRAINT "CK_Genres_ColorG" CHECK ("ColorG" IS NULL OR ("ColorG" >= 0 AND "ColorG" <= 255)),
                          CONSTRAINT "CK_Genres_ColorB" CHECK ("ColorB" IS NULL OR ("ColorB" >= 0 AND "ColorB" <= 255)),
                          CONSTRAINT "CK_Genres_RGB_AllOrNone" CHECK (
                              ("ColorR" IS NULL AND "ColorG" IS NULL AND "ColorB" IS NULL) OR
                              ("ColorR" IS NOT NULL AND "ColorG" IS NOT NULL AND "ColorB" IS NOT NULL)
                              )
);

-- Create GenreRelations table
CREATE TABLE "GenreRelations" (
                                  "GenreId" VARCHAR(128) NOT NULL,
                                  "RelatedGenreId" VARCHAR(128) NOT NULL,
                                  "Influence" INTEGER NOT NULL DEFAULT 5,
                                  "MGPC" REAL NOT NULL DEFAULT 0.0,
                                  CONSTRAINT "PK_GenreRelations" PRIMARY KEY ("GenreId", "RelatedGenreId"),
                                  CONSTRAINT "FK_GenreRelations_Genre" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT,
                                  CONSTRAINT "FK_GenreRelations_RelatedGenre" FOREIGN KEY ("RelatedGenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT,
                                  CONSTRAINT "CK_GenreRelations_Influence" CHECK ("Influence" >= 1 AND "Influence" <= 10),
                                  CONSTRAINT "CK_GenreRelations_MGPC" CHECK ("MGPC" >= 0.0 AND "MGPC" <= 1.0),
                                  CONSTRAINT "CK_GenreRelations_NoSelfReference" CHECK ("GenreId" != "RelatedGenreId")
);

-- Create Artists table
CREATE TABLE "Artists" (
                           "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                           "Name" VARCHAR(100) NOT NULL,
                           "Biography" VARCHAR(2000),
                           "OriginCountry" VARCHAR(100) NOT NULL,
                           "ActivityYears" TEXT,
                           "IsActive" BOOLEAN NOT NULL DEFAULT true,
                           "TimeStamp" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                           "CoverImageUrl" TEXT,
                           CONSTRAINT "PK_Artists" PRIMARY KEY ("Id"),
                           CONSTRAINT "UQ_Artists_Name" UNIQUE ("Name")
);

-- Create ArtistGenre junction table
CREATE TABLE "ArtistGenre" (
                               "ArtistId" VARCHAR(128) NOT NULL,
                               "GenreId" VARCHAR(128) NOT NULL,
                               "InfluenceCoefficient" REAL NOT NULL DEFAULT 1.0,
                               "AssociatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                               CONSTRAINT "PK_ArtistGenre" PRIMARY KEY ("ArtistId", "GenreId"),
                               CONSTRAINT "FK_ArtistGenre_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
                               CONSTRAINT "FK_ArtistGenre_Genre" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

-- Create ArtistSubgenre junction table
CREATE TABLE "ArtistSubgenre" (
                                  "ArtistId" VARCHAR(128) NOT NULL,
                                  "GenreId" VARCHAR(128) NOT NULL,
                                  "InfluenceCoefficient" REAL NOT NULL DEFAULT 1.0,
                                  "AssociatedDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                                  CONSTRAINT "PK_ArtistSubgenre" PRIMARY KEY ("ArtistId", "GenreId"),
                                  CONSTRAINT "FK_ArtistSubgenre_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
                                  CONSTRAINT "FK_ArtistSubgenre_Genre" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

-- Create ArtistMembers table
CREATE TABLE "ArtistMembers" (
                                 "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                                 "ArtistId" VARCHAR(128) NOT NULL,
                                 "FullName" VARCHAR(200) NOT NULL,
                                 "Instrument" VARCHAR(100),
                                 "ActivityPeriod" VARCHAR(100),
                                 "IsActive" BOOLEAN NOT NULL DEFAULT true,
                                 "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                                 CONSTRAINT "PK_ArtistMembers" PRIMARY KEY ("Id"),
                                 CONSTRAINT "FK_ArtistMembers_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

-- Create Albums table
CREATE TABLE "Albums" (
                          "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                          "ArtistId" VARCHAR(128) NOT NULL,
                          "Title" VARCHAR(200) NOT NULL,
                          "ReleaseDate" DATE,
                          "CoverImageUrl" TEXT,
                          "DurationSeconds" INTEGER,
                          "IsActive" BOOLEAN NOT NULL DEFAULT true,
                          "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                          CONSTRAINT "PK_Albums" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Albums_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

-- Create Comments table
CREATE TABLE "Comments" (
                            "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                            "ArtistId" VARCHAR(128) NOT NULL,
                            "Content" TEXT NOT NULL,
                            "AuthorName" VARCHAR(100) NOT NULL,
                            "IsActive" BOOLEAN NOT NULL DEFAULT true,
                            "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                            CONSTRAINT "PK_Comments" PRIMARY KEY ("Id"),
                            CONSTRAINT "FK_Comments_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

-- Create Photos table
CREATE TABLE "Photos" (
                          "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                          "ArtistId" VARCHAR(128) NOT NULL,
                          "ImageUrl" TEXT NOT NULL,
                          "Caption" VARCHAR(500),
                          "IsActive" BOOLEAN NOT NULL DEFAULT true,
                          "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                          CONSTRAINT "PK_Photos" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Photos_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

-- Create Events table
CREATE TABLE "Events" (
                          "Id" VARCHAR(128) NOT NULL DEFAULT gen_random_uuid()::text,
                          "ArtistId" VARCHAR(128) NOT NULL,
                          "Title" VARCHAR(200) NOT NULL,
                          "Description" TEXT,
                          "Venue" VARCHAR(200),
                          "City" VARCHAR(100),
                          "Country" VARCHAR(100),
                          "EventDate" TIMESTAMP WITH TIME ZONE NOT NULL,
                          "IsActive" BOOLEAN NOT NULL DEFAULT true,
                          "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                          CONSTRAINT "PK_Events" PRIMARY KEY ("Id"),
                          CONSTRAINT "FK_Events_Artist" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);


-- Create Fanaticos table
CREATE TABLE "Fanaticos" (
                             "Id" VARCHAR(50) NOT NULL DEFAULT ('F-' || substr(gen_random_uuid()::text, 1, 12)),
                             "NombreUsuario" VARCHAR(30) NOT NULL,
                             "Nombre" VARCHAR(100) NOT NULL,
                             "Contrasena" VARCHAR(12) NOT NULL,
                             "Pais" VARCHAR(100) NOT NULL,
                             "Avatar" VARCHAR(200) NOT NULL,
                             "FechaRegistro" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                             CONSTRAINT "PK_Fanaticos" PRIMARY KEY ("Id"),
                             CONSTRAINT "UQ_Fanaticos_NombreUsuario" UNIQUE ("NombreUsuario")
);


-- Create FanaticoGenres join table
CREATE TABLE "FanaticoGenres" (
                                  "FanaticoId" VARCHAR(50) NOT NULL,
                                  "GenreId" VARCHAR(128) NOT NULL,
                                  CONSTRAINT "PK_FanaticoGenres" PRIMARY KEY ("FanaticoId", "GenreId"),
                                  CONSTRAINT "FK_FanaticoGenres_Fanatico" FOREIGN KEY ("FanaticoId") REFERENCES "Fanaticos"("Id") ON DELETE CASCADE,
                                  CONSTRAINT "FK_FanaticoGenres_Genre" FOREIGN KEY ("GenreId") REFERENCES "Genres"("Id") ON DELETE CASCADE
);


-- ============================================================================
-- PART 4: INDEXES FOR PERFORMANCE
-- ============================================================================

-- Indexes on Artists
CREATE INDEX "IX_Artists_Name" ON "Artists" ("Name");
CREATE INDEX "IX_Artists_OriginCountry" ON "Artists" ("OriginCountry");
CREATE INDEX "IX_Artists_IsActive" ON "Artists" ("IsActive");

-- Indexes on Genres
CREATE INDEX "IX_Genres_Name" ON "Genres" ("Name");
CREATE INDEX "IX_Genres_ClusterId" ON "Genres" ("ClusterId");
CREATE INDEX "IX_Genres_ParentGenreId" ON "Genres" ("ParentGenreId");
CREATE INDEX "IX_Genres_IsActive" ON "Genres" ("IsActive");
CREATE INDEX "IX_Genres_IsSubgenre" ON "Genres" ("IsSubgenre");

-- Indexes on junction tables
CREATE INDEX "IX_ArtistGenre_GenreId" ON "ArtistGenre" ("GenreId");
CREATE INDEX "IX_ArtistSubgenre_GenreId" ON "ArtistSubgenre" ("GenreId");

-- Indexes on related tables
CREATE INDEX "IX_Albums_ArtistId" ON "Albums" ("ArtistId");
CREATE INDEX "IX_Comments_ArtistId" ON "Comments" ("ArtistId");
CREATE INDEX "IX_Photos_ArtistId" ON "Photos" ("ArtistId");
CREATE INDEX "IX_Events_ArtistId" ON "Events" ("ArtistId");
CREATE INDEX "IX_Events_EventDate" ON "Events" ("EventDate");

-- ============================================================================
-- PART 5: SAMPLE DATA INSERTION FOR TESTING
-- ============================================================================

-- Insert sample clusters
INSERT INTO "Clusters" ("Id", "Name", "Description") VALUES
                                                         ('cluster-rock', 'Rock', 'All rock-related genres and subgenres'),
                                                         ('cluster-electronic', 'Electronic', 'Electronic music genres and variations'),
                                                         ('cluster-classical', 'Classical', 'Classical and orchestral music'),
                                                         ('cluster-folk', 'Folk', 'Traditional and folk music from around the world'),
                                                         ('cluster-jazz', 'Jazz', 'Jazz and its many variations');

-- Insert sample main genres
INSERT INTO "Genres" ("Id", "Name", "Description", "ClusterId", "GenreOriginCountry", "ColorR", "ColorG", "ColorB", "Bpm", "BpmLower", "BpmUpper", "AvrgDuration", "GenreCreationYear", "GenreTipicalMode") VALUES
                                                                                                                                                                                                                ('genre-rock', 'Rock', 'Classic rock music', 'cluster-rock', 'United States', 255, 0, 0, 120, 100, 140,20,1970, 0),
                                                                                                                                                                                                                ('genre-electronic', 'Electronic', 'Electronic dance music', 'cluster-electronic', 'Germany', 0, 255, 0, 128, 120, 140, 20,2000, 1),
                                                                                                                                                                                                                ('genre-classical', 'Classical', 'Classical orchestral music', 'cluster-classical', 'Austria', 0, 0, 255, 80, 60, 100,30,1600, 1),
                                                                                                                                                                                                                ('genre-folk', 'Folk', 'Traditional folk music', 'cluster-folk', 'Various', 255, 255, 0, 100, 80, 120,30,1970, 1),
                                                                                                                                                                                                                ('genre-jazz', 'Jazz', 'Jazz music', 'cluster-jazz', 'United States', 255, 0, 255, 120, 100, 160,30,1950, 0);

-- Insert sample subgenres
INSERT INTO "Genres" ("Id", "Name", "Description", "ParentGenreId", "IsSubgenre", "Bpm", "BpmLower", "BpmUpper","AvrgDuration") VALUES
                                                                                                                                    ('subgenre-hard-rock', 'Hard Rock', 'Harder variation of rock', 'genre-rock', true, 130, 120, 150, 100),
                                                                                                                                    ('subgenre-prog-rock', 'Progressive Rock', 'Complex rock with unusual time signatures', 'genre-rock', true, 110, 80, 140, 2000),
                                                                                                                                    ('subgenre-house', 'House', 'House music subgenre of electronic', 'genre-electronic', true, 128, 120, 135, 3000),
                                                                                                                                    ('subgenre-techno', 'Techno', 'Techno subgenre of electronic', 'genre-electronic', true, 130, 125, 140, 400),
                                                                                                                                    ('subgenre-bebop', 'Bebop', 'Fast-paced jazz style', 'genre-jazz', true, 140, 120, 180, 500);

-- Insert sample genre relations
INSERT INTO "GenreRelations" ("GenreId", "RelatedGenreId", "Influence", "MGPC") VALUES
                                                                                    ('genre-rock', 'genre-folk', 7, 0.3),
                                                                                    ('genre-jazz', 'genre-classical', 8, 0.4),
                                                                                    ('genre-electronic', 'genre-jazz', 5, 0.2),
                                                                                    ('subgenre-hard-rock', 'genre-rock', 9, 0.8),
                                                                                    ('subgenre-prog-rock', 'genre-classical', 6, 0.3);

-- Insert sample artists
INSERT INTO "Artists" ("Id", "Name", "Biography", "OriginCountry", "ActivityYears") VALUES
                                                                                        ('artist-beatles', 'The Beatles', 'Legendary British rock band', 'United Kingdom', '1960–1970'),
                                                                                        ('artist-kraftwerk', 'Kraftwerk', 'German electronic music pioneers', 'Germany', '1970–presente'),
                                                                                        ('artist-mozart', 'Wolfgang Amadeus Mozart', 'Classical composer', 'Austria', '1756–1791'),
                                                                                        ('artist-dylan', 'Bob Dylan', 'American folk singer-songwriter', 'United States', '1961–presente'),
                                                                                        ('artist-davis', 'Miles Davis', 'Jazz trumpeter and composer', 'United States', '1944–1991');

-- Insert sample artist-genre associations
INSERT INTO "ArtistGenre" ("ArtistId", "GenreId", "InfluenceCoefficient") VALUES
                                                                              ('artist-beatles', 'genre-rock', 1.0),
                                                                              ('artist-kraftwerk', 'genre-electronic', 1.0),
                                                                              ('artist-mozart', 'genre-classical', 1.0),
                                                                              ('artist-dylan', 'genre-folk', 1.0),
                                                                              ('artist-davis', 'genre-jazz', 1.0);

-- Insert sample artist-subgenre associations
INSERT INTO "ArtistSubgenre" ("ArtistId", "GenreId", "InfluenceCoefficient") VALUES
                                                                                 ('artist-beatles', 'subgenre-hard-rock', 0.7),
                                                                                 ('artist-kraftwerk', 'subgenre-house', 0.8),
                                                                                 ('artist-davis', 'subgenre-bebop', 0.9);

-- Insert sample albums
INSERT INTO "Albums" ("ArtistId", "Title", "ReleaseDate", "DurationSeconds") VALUES
                                                                                 ('artist-beatles', 'Abbey Road', '1969-09-26', 2873),
                                                                                 ('artist-kraftwerk', 'Autobahn', '1974-11-01', 2567),
                                                                                 ('artist-mozart', 'Symphony No. 40', '1788-07-25', 1980),
                                                                                 ('artist-dylan', 'Highway 61 Revisited', '1965-08-30', 3120),
                                                                                 ('artist-davis', 'Kind of Blue', '1959-08-17', 2760);

-- ============================================================================
-- PART 6: VERIFICATION QUERIES
-- ============================================================================

-- Verify the setup
SELECT 'Clusters' as table_name, COUNT(*) as record_count FROM "Clusters"
UNION ALL
SELECT 'Genres', COUNT(*) FROM "Genres"
UNION ALL
SELECT 'GenreRelations', COUNT(*) FROM "GenreRelations"
UNION ALL
SELECT 'Artists', COUNT(*) FROM "Artists"
UNION ALL
SELECT 'ArtistGenre', COUNT(*) FROM "ArtistGenre"
UNION ALL
SELECT 'ArtistSubgenre', COUNT(*) FROM "ArtistSubgenre"
UNION ALL
SELECT 'Albums', COUNT(*) FROM "Albums";

-- Show sample data
SELECT 'Sample Clusters:' as info;
SELECT "Name", "Description" FROM "Clusters" ORDER BY "Name";

SELECT 'Sample Genres:' as info;
SELECT "Name", "Description", "IsSubgenre" FROM "Genres" ORDER BY "IsSubgenre", "Name";

SELECT 'Sample Artists:' as info;
SELECT "Name", "OriginCountry", "ActivityYears" FROM "Artists" ORDER BY "Name";


SELECT * FROM "Genres";

SELECT * FROM "Artists";

SELECT * FROM "Fanaticos";
