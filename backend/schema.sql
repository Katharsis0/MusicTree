CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Artists" (
    "Id" character varying(128) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Biography" character varying(2000),
    "OriginCountry" character varying(100) NOT NULL,
    "ActivityYears" character varying(200) NOT NULL,
    "CoverImageUrl" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "TimeStamp" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_Artists" PRIMARY KEY ("Id")
);

CREATE TABLE "Clusters" (
    "Id" character varying(128) NOT NULL,
    "Name" character varying(30) NOT NULL,
    "Description" character varying(300),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "TimeStamp" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_Clusters" PRIMARY KEY ("Id")
);

CREATE TABLE "Album" (
    "Id" text NOT NULL,
    "ArtistId" character varying(128) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "ReleaseDate" timestamp with time zone NOT NULL,
    "CoverImageUrl" character varying(500),
    "DurationSeconds" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Album" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Album_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ArtistMember" (
    "Id" text NOT NULL,
    "ArtistId" character varying(128) NOT NULL,
    "FullName" character varying(100) NOT NULL,
    "Instrument" character varying(100) NOT NULL,
    "ActivityPeriod" character varying(200) NOT NULL,
    "IsActive" boolean NOT NULL,
    "JoinedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ArtistMember" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ArtistMember_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Comment" (
    "Id" text NOT NULL,
    "ArtistId" character varying(128) NOT NULL,
    "Content" character varying(1000) NOT NULL,
    "AuthorName" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Comment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Comment_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Event" (
    "Id" text NOT NULL,
    "ArtistId" character varying(128) NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(500),
    "Venue" character varying(200) NOT NULL,
    "City" character varying(100) NOT NULL,
    "Country" character varying(100) NOT NULL,
    "EventDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Event" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Event_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Photo" (
    "Id" text NOT NULL,
    "ArtistId" character varying(128) NOT NULL,
    "ImageUrl" character varying(500) NOT NULL,
    "Caption" character varying(200),
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Photo" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Photo_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Genres" (
    "Id" character varying(128) NOT NULL,
    "Name" character varying(30) NOT NULL,
    "Description" character varying(1000),
    "IsSubgenre" boolean NOT NULL,
    "ParentGenreId" character varying(128),
    "ClusterId" character varying(128),
    "Key" integer NOT NULL DEFAULT -1,
    "BpmLower" integer NOT NULL,
    "BpmUpper" integer NOT NULL,
    "Bpm" integer NOT NULL DEFAULT 120,
    "GenreTipicalMode" real NOT NULL,
    "Volume" integer NOT NULL DEFAULT -20,
    "CompasMetric" integer NOT NULL DEFAULT 4,
    "AvrgDuration" integer NOT NULL,
    "ColorR" integer,
    "ColorG" integer,
    "ColorB" integer,
    "GenreCreationYear" integer,
    "GenreOriginCountry" character varying(100),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "TimeStamp" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_Genres" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Genres_ColorB" CHECK (ColorB IS NULL OR (ColorB >= 0 AND ColorB <= 255)),
    CONSTRAINT "CK_Genres_ColorG" CHECK (ColorG IS NULL OR (ColorG >= 0 AND ColorG <= 255)),
    CONSTRAINT "CK_Genres_ColorR" CHECK (ColorR IS NULL OR (ColorR >= 0 AND ColorR <= 255)),
    CONSTRAINT "CK_Genres_RGB_AllOrNone" CHECK ((ColorR IS NULL AND ColorG IS NULL AND ColorB IS NULL) OR (ColorR IS NOT NULL AND ColorG IS NOT NULL AND ColorB IS NOT NULL)),
    CONSTRAINT "FK_Genres_Clusters_ClusterId" FOREIGN KEY ("ClusterId") REFERENCES "Clusters" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Genres_Genres_ParentGenreId" FOREIGN KEY ("ParentGenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ArtistGenre" (
    "ArtistId" character varying(128) NOT NULL,
    "GenreId" character varying(128) NOT NULL,
    "InfluenceCoefficient" real NOT NULL DEFAULT 1,
    "AssociatedDate" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_ArtistGenre" PRIMARY KEY ("ArtistId", "GenreId"),
    CONSTRAINT "CK_ArtistGenre_NotSubgenre" CHECK (NOT EXISTS (SELECT 1 FROM "Genres" WHERE "Id" = "GenreId" AND "IsSubgenre" = true)),
    CONSTRAINT "FK_ArtistGenre_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ArtistGenre_Genres_GenreId" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ArtistSubgenre" (
    "ArtistId" character varying(128) NOT NULL,
    "GenreId" character varying(128) NOT NULL,
    "InfluenceCoefficient" real NOT NULL DEFAULT 1,
    "AssociatedDate" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_ArtistSubgenre" PRIMARY KEY ("ArtistId", "GenreId"),
    CONSTRAINT "CK_ArtistSubgenre_IsSubgenre" CHECK (EXISTS (SELECT 1 FROM "Genres" WHERE "Id" = "GenreId" AND "IsSubgenre" = true)),
    CONSTRAINT "FK_ArtistSubgenre_Artists_ArtistId" FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ArtistSubgenre_Genres_GenreId" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "GenreRelations" (
    "GenreId" character varying(128) NOT NULL,
    "RelatedGenreId" character varying(128) NOT NULL,
    "Influence" integer NOT NULL DEFAULT 5,
    "MGPC" real NOT NULL DEFAULT 0,
    CONSTRAINT "PK_GenreRelations" PRIMARY KEY ("GenreId", "RelatedGenreId"),
    CONSTRAINT "CK_GenreRelations_Influence" CHECK ([Influence] >= 1 AND [Influence] <= 10),
    CONSTRAINT "CK_GenreRelations_MGPC" CHECK ([MGPC] >= 0.0 AND [MGPC] <= 1.0),
    CONSTRAINT "CK_GenreRelations_NoSelfReference" CHECK ([GenreId] != [RelatedGenreId]),
    CONSTRAINT "FK_GenreRelations_Genres_GenreId" FOREIGN KEY ("GenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GenreRelations_Genres_RelatedGenreId" FOREIGN KEY ("RelatedGenreId") REFERENCES "Genres" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Album_ArtistId" ON "Album" ("ArtistId");

CREATE INDEX "IX_ArtistGenre_GenreId" ON "ArtistGenre" ("GenreId");

CREATE INDEX "IX_ArtistMember_ArtistId" ON "ArtistMember" ("ArtistId");

CREATE INDEX "IX_Artists_IsActive" ON "Artists" ("IsActive");

CREATE INDEX "IX_Artists_Name" ON "Artists" ("Name");

CREATE INDEX "IX_Artists_OriginCountry" ON "Artists" ("OriginCountry");

CREATE INDEX "IX_ArtistSubgenre_GenreId" ON "ArtistSubgenre" ("GenreId");

CREATE INDEX "IX_Comment_ArtistId" ON "Comment" ("ArtistId");

CREATE INDEX "IX_Event_ArtistId" ON "Event" ("ArtistId");

CREATE INDEX "IX_GenreRelations_RelatedGenreId" ON "GenreRelations" ("RelatedGenreId");

CREATE INDEX "IX_Genres_ClusterId" ON "Genres" ("ClusterId");

CREATE INDEX "IX_Genres_ParentGenreId" ON "Genres" ("ParentGenreId");

CREATE INDEX "IX_Photo_ArtistId" ON "Photo" ("ArtistId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250605213710_AddColorComponentsToGenres', '9.0.5');

COMMIT;

