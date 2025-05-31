-- MusicTree Database Initialization Script
-- Project: MusicTree para X-Tec
-- Sprint 1 Implementation - Backend & Database with RGB Color Support
-- Author: Katharsis, AudioSignal Labs.
-- Date: May 2025

-- =============================================================================
-- DATABASE AND USER SETUP (handled by setup script)
-- =============================================================================

-- Connect to musictree database and run the following as musictree_admin
-- \c musictree;

-- =============================================================================
-- SCHEMA CREATION
-- =============================================================================

-- Drop tables if they exist (for clean reinstallation)
DROP TABLE IF EXISTS "GenreRelations" CASCADE;
DROP TABLE IF EXISTS "Genres" CASCADE;
DROP TABLE IF EXISTS "Clusters" CASCADE;
DROP TABLE IF EXISTS "Artists" CASCADE;

-- Drop views if they exist
DROP VIEW IF EXISTS "ActiveGenresWithClusters" CASCADE;
DROP VIEW IF EXISTS "GenreRelationshipsWithNames" CASCADE;

-- Drop functions if they exist
DROP FUNCTION IF EXISTS GetGenreHierarchy(TEXT) CASCADE;
DROP FUNCTION IF EXISTS rgb_to_hex(INTEGER, INTEGER, INTEGER) CASCADE;
DROP FUNCTION IF EXISTS hex_to_rgb(TEXT) CASCADE;

-- =============================================================================
-- TABLE: Clusters
-- Description: Music genre clusters for categorization
-- =============================================================================
CREATE TABLE "Clusters" (
                            "Id"          TEXT                     NOT NULL,
                            "Name"        VARCHAR(30)              NOT NULL,
                            "Description" VARCHAR(300),
                            "IsActive"    BOOLEAN                  NOT NULL DEFAULT TRUE,
                            "TimeStamp"   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

                            CONSTRAINT "PK_Clusters" PRIMARY KEY ("Id"),
                            CONSTRAINT "UQ_Clusters_Name" UNIQUE ("Name")
);

-- Add table comment
COMMENT ON TABLE "Clusters" IS 'Music genre clusters for high-level categorization';
COMMENT ON COLUMN "Clusters"."Id" IS 'Unique cluster identifier';
COMMENT ON COLUMN "Clusters"."Name" IS 'Display name of the cluster';
COMMENT ON COLUMN "Clusters"."Description" IS 'Detailed description of the cluster';
COMMENT ON COLUMN "Clusters"."IsActive" IS 'Indicates if cluster is currently active';
COMMENT ON COLUMN "Clusters"."TimeStamp" IS 'Record creation/modification timestamp';

-- =============================================================================
-- TABLE: Artists
-- Description: Music artists
-- =============================================================================
CREATE TABLE "Artists" (
                           "Id"        TEXT                     NOT NULL,
                           "Name"      VARCHAR(100)             NOT NULL,
                           "Biography" VARCHAR(2000),
                           "IsActive"  BOOLEAN                  NOT NULL DEFAULT TRUE,
                           "TimeStamp" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

                           CONSTRAINT "PK_Artists" PRIMARY KEY ("Id"),
                           CONSTRAINT "UQ_Artists_Name" UNIQUE ("Name")
);

-- Add table comments
COMMENT ON TABLE "Artists" IS 'Music artists information';
COMMENT ON COLUMN "Artists"."Id" IS 'Unique artist identifier';
COMMENT ON COLUMN "Artists"."Name" IS 'Artist name';
COMMENT ON COLUMN "Artists"."Biography" IS 'Artist biography';
COMMENT ON COLUMN "Artists"."IsActive" IS 'Indicates if artist is currently active';
COMMENT ON COLUMN "Artists"."TimeStamp" IS 'Record creation/modification timestamp';

-- =============================================================================
-- TABLE: Genres
-- Description: Music genres with detailed characteristics and RGB color support
-- =============================================================================
CREATE TABLE "Genres" (
                          "Id"                 TEXT                                NOT NULL,
                          "Name"               VARCHAR(30)                         NOT NULL,
                          "Description"        VARCHAR(1000),
                          "IsSubgenre"         BOOLEAN                             NOT NULL DEFAULT FALSE,
                          "ParentGenreId"      TEXT,
                          "ClusterId"          TEXT,
                          "Key"                INTEGER   DEFAULT -1                NOT NULL,
                          "BpmLower"           INTEGER                             NOT NULL,
                          "BpmUpper"           INTEGER                             NOT NULL,
                          "Bpm"                INTEGER   DEFAULT 120               NOT NULL,

    -- RGB Color components (replacing hex color)
                          "ColorR"             INTEGER,
                          "ColorG"             INTEGER,
                          "ColorB"             INTEGER,

                          "GenreCreationYear"  INTEGER,
                          "GenreOriginCountry" VARCHAR(100),
                          "GenreTipicalMode"   REAL                                NOT NULL,
                          "Volume"             INTEGER   DEFAULT -20               NOT NULL,
                          "CompasMetric"       INTEGER   DEFAULT 4                 NOT NULL,
                          "AvrgDuration"       INTEGER                             NOT NULL,
                          "IsActive"           BOOLEAN   DEFAULT TRUE              NOT NULL,
                          "TimeStamp"          TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,

    -- Primary Key
                          CONSTRAINT "PK_Genres" PRIMARY KEY ("Id"),

    -- Foreign Keys
                          CONSTRAINT "FK_Genres_ParentGenre"
                              FOREIGN KEY ("ParentGenreId") REFERENCES "Genres"("Id") ON DELETE RESTRICT,
                          CONSTRAINT "FK_Genres_Cluster"
                              FOREIGN KEY ("ClusterId") REFERENCES "Clusters"("Id") ON DELETE SET NULL,

    -- Check Constraints
                          CONSTRAINT "CK_Genres_Key"
                              CHECK ("Key" >= -1 AND "Key" <= 11),
                          CONSTRAINT "CK_Genres_BpmLower"
                              CHECK ("BpmLower" >= 0 AND "BpmLower" <= 250),
                          CONSTRAINT "CK_Genres_BpmUpper"
                              CHECK ("BpmUpper" >= 0 AND "BpmUpper" <= 250),
                          CONSTRAINT "CK_Genres_Bpm"
                              CHECK ("Bpm" >= 0 AND "Bpm" <= 250),
                          CONSTRAINT "CK_Genres_BpmRange"
                              CHECK ("BpmLower" <= "BpmUpper"),
                          CONSTRAINT "CK_Genres_GenreTipicalMode"
                              CHECK ("GenreTipicalMode" >= 0.0 AND "GenreTipicalMode" <= 1.0),
                          CONSTRAINT "CK_Genres_Volume"
                              CHECK ("Volume" >= -60 AND "Volume" <= 0),
                          CONSTRAINT "CK_Genres_CompasMetric"
                              CHECK ("CompasMetric" >= 0 AND "CompasMetric" <= 8),
                          CONSTRAINT "CK_Genres_AvrgDuration"
                              CHECK ("AvrgDuration" >= 0 AND "AvrgDuration" <= 3600),

    -- RGB Color constraints
                          CONSTRAINT "CK_Genres_ColorR"
                              CHECK ("ColorR" IS NULL OR ("ColorR" >= 0 AND "ColorR" <= 255)),
                          CONSTRAINT "CK_Genres_ColorG"
                              CHECK ("ColorG" IS NULL OR ("ColorG" >= 0 AND "ColorG" <= 255)),
                          CONSTRAINT "CK_Genres_ColorB"
                              CHECK ("ColorB" IS NULL OR ("ColorB" >= 0 AND "ColorB" <= 255)),

    -- All RGB components must be provided together or none at all
                          CONSTRAINT "CK_Genres_RGB_AllOrNone"
                              CHECK (
                                  ("ColorR" IS NULL AND "ColorG" IS NULL AND "ColorB" IS NULL) OR
                                  ("ColorR" IS NOT NULL AND "ColorG" IS NOT NULL AND "ColorB" IS NOT NULL)
                                  ),

    -- Unique Constraints
                          CONSTRAINT "UQ_Genres_Name" UNIQUE ("Name")
);

-- Add table comments
COMMENT ON TABLE "Genres" IS 'Music genres with detailed musical characteristics and RGB colors';
COMMENT ON COLUMN "Genres"."Id" IS 'Unique genre identifier';
COMMENT ON COLUMN "Genres"."Name" IS 'Genre display name';
COMMENT ON COLUMN "Genres"."Description" IS 'Detailed genre description';
COMMENT ON COLUMN "Genres"."IsSubgenre" IS 'Indicates if this is a subgenre';
COMMENT ON COLUMN "Genres"."ParentGenreId" IS 'Reference to parent genre if subgenre';
COMMENT ON COLUMN "Genres"."ClusterId" IS 'Reference to genre cluster';
COMMENT ON COLUMN "Genres"."Key" IS 'Musical key (-1 for any, 0-11 for chromatic scale)';
COMMENT ON COLUMN "Genres"."BpmLower" IS 'Lower BPM range boundary';
COMMENT ON COLUMN "Genres"."BpmUpper" IS 'Upper BPM range boundary';
COMMENT ON COLUMN "Genres"."Bpm" IS 'Typical BPM for the genre';
COMMENT ON COLUMN "Genres"."ColorR" IS 'Red component of RGB color (0-255)';
COMMENT ON COLUMN "Genres"."ColorG" IS 'Green component of RGB color (0-255)';
COMMENT ON COLUMN "Genres"."ColorB" IS 'Blue component of RGB color (0-255)';
COMMENT ON COLUMN "Genres"."GenreCreationYear" IS 'Year when genre was created/recognized';
COMMENT ON COLUMN "Genres"."GenreOriginCountry" IS 'Country of origin';
COMMENT ON COLUMN "Genres"."GenreTipicalMode" IS 'Typical musical mode (0.0 = minor, 1.0 = major)';
COMMENT ON COLUMN "Genres"."Volume" IS 'Typical volume level in dB';
COMMENT ON COLUMN "Genres"."CompasMetric" IS 'Time signature numerator';
COMMENT ON COLUMN "Genres"."AvrgDuration" IS 'Average song duration in seconds';

-- =============================================================================
-- TABLE: GenreRelations
-- Description: Relationships between genres with influence metrics
-- =============================================================================
CREATE TABLE "GenreRelations" (
                                  "GenreId"        TEXT              NOT NULL,
                                  "RelatedGenreId" TEXT              NOT NULL,
                                  "Influence"      INTEGER DEFAULT 5 NOT NULL,
                                  "MGPC"           REAL    DEFAULT 0 NOT NULL,

    -- Primary Key
                                  CONSTRAINT "PK_GenreRelations" PRIMARY KEY ("GenreId", "RelatedGenreId"),

    -- Foreign Keys
                                  CONSTRAINT "FK_GenreRelations_Genre"
                                      FOREIGN KEY ("GenreId") REFERENCES "Genres"("Id") ON DELETE RESTRICT,
                                  CONSTRAINT "FK_GenreRelations_RelatedGenre"
                                      FOREIGN KEY ("RelatedGenreId") REFERENCES "Genres"("Id") ON DELETE RESTRICT,

    -- Check Constraints
                                  CONSTRAINT "CK_GenreRelations_Influence"
                                      CHECK ("Influence" >= 1 AND "Influence" <= 10),
                                  CONSTRAINT "CK_GenreRelations_MGPC"
                                      CHECK ("MGPC" >= 0.0 AND "MGPC" <= 1.0),

    -- Prevent self-references
                                  CONSTRAINT "CK_GenreRelations_NoSelfReference"
                                      CHECK ("GenreId" != "RelatedGenreId")
    );

-- Add table comments
COMMENT ON TABLE "GenreRelations" IS 'Relationships between music genres with influence metrics';
COMMENT ON COLUMN "GenreRelations"."GenreId" IS 'Source genre identifier';
COMMENT ON COLUMN "GenreRelations"."RelatedGenreId" IS 'Related genre identifier';
COMMENT ON COLUMN "GenreRelations"."Influence" IS 'Influence level (1-10 scale)';
COMMENT ON COLUMN "GenreRelations"."MGPC" IS 'Music Genre Proximity Coefficient (0.0-1.0)';

-- =============================================================================
-- INDEXES FOR PERFORMANCE
-- =============================================================================

-- Clusters indexes
CREATE INDEX "IX_Clusters_Name" ON "Clusters"("Name");
CREATE INDEX "IX_Clusters_IsActive" ON "Clusters"("IsActive");
CREATE INDEX "IX_Clusters_TimeStamp" ON "Clusters"("TimeStamp");

-- Artists indexes
CREATE INDEX "IX_Artists_Name" ON "Artists"("Name");
CREATE INDEX "IX_Artists_IsActive" ON "Artists"("IsActive");

-- Genres indexes
CREATE INDEX "IX_Genres_Name" ON "Genres"("Name");
CREATE INDEX "IX_Genres_IsSubgenre" ON "Genres"("IsSubgenre");
CREATE INDEX "IX_Genres_ParentGenreId" ON "Genres"("ParentGenreId");
CREATE INDEX "IX_Genres_ClusterId" ON "Genres"("ClusterId");
CREATE INDEX "IX_Genres_IsActive" ON "Genres"("IsActive");
CREATE INDEX "IX_Genres_BpmRange" ON "Genres"("BpmLower", "BpmUpper");
CREATE INDEX "IX_Genres_CompasMetric" ON "Genres"("CompasMetric");
CREATE INDEX "IX_Genres_CreationYear" ON "Genres"("GenreCreationYear");
CREATE INDEX "IX_Genres_OriginCountry" ON "Genres"("GenreOriginCountry");

-- GenreRelations indexes
CREATE INDEX "IX_GenreRelations_RelatedGenreId" ON "GenreRelations"("RelatedGenreId");
CREATE INDEX "IX_GenreRelations_Influence" ON "GenreRelations"("Influence");
CREATE INDEX "IX_GenreRelations_MGPC" ON "GenreRelations"("MGPC");

-- =============================================================================
-- SAMPLE DATA WITH RGB COLORS
-- =============================================================================

-- Insert sample clusters
INSERT INTO "Clusters" ("Id", "Name", "Description", "IsActive") VALUES
                                                                     ('C-ELECTRONIC001', 'Electronic', 'Electronic music and derivatives', true),
                                                                     ('C-ROCK00000001', 'Rock', 'Rock music and its subgenres', true),
                                                                     ('C-POP000000001', 'Pop', 'Popular music genres', true),
                                                                     ('C-CLASSICAL001', 'Classical', 'Classical and orchestral music', true),
                                                                     ('C-WORLD0000001', 'World', 'Traditional and world music', true);

-- Insert sample genres with RGB colors
INSERT INTO "Genres" ("Id", "Name", "Description", "IsSubgenre", "ParentGenreId", "ClusterId",
                      "Key", "BpmLower", "BpmUpper", "Bpm", "ColorR", "ColorG", "ColorB",
                      "GenreCreationYear", "GenreOriginCountry", "GenreTipicalMode", "Volume",
                      "CompasMetric", "AvrgDuration") VALUES
                                                          ('G-ROCK00000001', 'Rock', 'Traditional rock music', false, null, 'C-ROCK00000001',
                                                           -1, 120, 140, 130, 255, 107, 53, 1950, 'United States', 0.7, -15, 4, 240),
                                                          ('G-POP000000001', 'Pop', 'Popular mainstream music', false, null, 'C-POP000000001',
                                                           -1, 100, 130, 115, 255, 20, 147, 1950, 'United States', 0.8, -12, 4, 210),
                                                          ('G-ELECTRONIC01', 'Electronic', 'Electronic dance music', false, null, 'C-ELECTRONIC001',
                                                           -1, 120, 150, 128, 0, 255, 255, 1970, 'Germany', 0.6, -18, 4, 300),
                                                          ('G-CLASSICAL001', 'Classical', 'Classical orchestral music', false, null, 'C-CLASSICAL001',
                                                           -1, 60, 120, 90, 139, 69, 19, 1600, 'Europe', 0.5, -25, 4, 420);

-- Insert sample genre relations with MGPC values
INSERT INTO "GenreRelations" ("GenreId", "RelatedGenreId", "Influence", "MGPC") VALUES
                                                                                    ('G-ROCK00000001', 'G-POP000000001', 7, 0.6),
                                                                                    ('G-POP000000001', 'G-ROCK00000001', 5, 0.4),
                                                                                    ('G-ELECTRONIC01', 'G-POP000000001', 6, 0.3),
                                                                                    ('G-CLASSICAL001', 'G-ROCK00000001', 3, 0.2);

-- =============================================================================
-- VIEWS WITH RGB COLOR SUPPORT
-- =============================================================================

-- View for active genres with cluster information and RGB colors
CREATE VIEW "ActiveGenresWithClusters" AS
SELECT
    g."Id",
    g."Name",
    g."Description",
    g."IsSubgenre",
    pg."Name" AS "ParentGenreName",
    c."Name" AS "ClusterName",
    g."Bpm",
    g."ColorR",
    g."ColorG",
    g."ColorB",
    CASE
        WHEN g."ColorR" IS NOT NULL AND g."ColorG" IS NOT NULL AND g."ColorB" IS NOT NULL THEN
            'rgb(' || g."ColorR" || ',' || g."ColorG" || ',' || g."ColorB" || ')'
        ELSE NULL
        END AS "RgbColor",
    CASE
        WHEN g."ColorR" IS NOT NULL AND g."ColorG" IS NOT NULL AND g."ColorB" IS NOT NULL THEN
            '#' || LPAD(TO_HEX(g."ColorR"), 2, '0') || LPAD(TO_HEX(g."ColorG"), 2, '0') || LPAD(TO_HEX(g."ColorB"), 2, '0')
        ELSE NULL
        END AS "HexColor",
    g."GenreCreationYear",
    g."GenreOriginCountry",
    g."CompasMetric",
    g."BpmLower",
    g."BpmUpper"
FROM "Genres" g
         LEFT JOIN "Genres" pg ON g."ParentGenreId" = pg."Id"
         LEFT JOIN "Clusters" c ON g."ClusterId" = c."Id"
WHERE g."IsActive" = true;

-- View for genre relationships with names
CREATE VIEW "GenreRelationshipsWithNames" AS
SELECT
    gr."GenreId",
    g1."Name" AS "GenreName",
    gr."RelatedGenreId",
    g2."Name" AS "RelatedGenreName",
    gr."Influence",
    gr."MGPC"
FROM "GenreRelations" gr
         JOIN "Genres" g1 ON gr."GenreId" = g1."Id"
         JOIN "Genres" g2 ON gr."RelatedGenreId" = g2."Id"
WHERE g1."IsActive" = true AND g2."IsActive" = true;

-- =============================================================================
-- FUNCTIONS FOR RGB/HEX CONVERSION
-- =============================================================================

-- Function to convert RGB to Hex
CREATE OR REPLACE FUNCTION rgb_to_hex(r INTEGER, g INTEGER, b INTEGER)
RETURNS TEXT AS $function$
BEGIN
    IF r IS NULL OR g IS NULL OR b IS NULL THEN
        RETURN NULL;
END IF;
    
    IF r < 0 OR r > 255 OR g < 0 OR g > 255 OR b < 0 OR b > 255 THEN
        RAISE EXCEPTION 'RGB values must be between 0 and 255';
END IF;

RETURN '#' || LPAD(TO_HEX(r), 2, '0') || LPAD(TO_HEX(g), 2, '0') || LPAD(TO_HEX(b), 2, '0');
END;
$function$ LANGUAGE plpgsql;

-- Function to extract RGB components from hex
CREATE OR REPLACE FUNCTION hex_to_rgb(hex_color TEXT)
RETURNS TABLE(r INTEGER, g INTEGER, b INTEGER) AS $function$
BEGIN
    IF hex_color IS NULL OR LENGTH(hex_color) != 7 OR SUBSTR(hex_color, 1, 1) != '#' THEN
        RAISE EXCEPTION 'Invalid hex color format. Expected #RRGGBB';
END IF;
    
    r := ('x' || SUBSTR(hex_color, 2, 2))::bit(8)::int;
    g := ('x' || SUBSTR(hex_color, 4, 2))::bit(8)::int;
    b := ('x' || SUBSTR(hex_color, 6, 2))::bit(8)::int;
    
    RETURN NEXT;
END;
$function$ LANGUAGE plpgsql;

-- Function to get genre hierarchy
CREATE OR REPLACE FUNCTION GetGenreHierarchy(input_genre_id TEXT)
RETURNS TABLE(
    level_num INTEGER,
    genre_id TEXT,
    genre_name VARCHAR(30),
    parent_id TEXT
) AS $
BEGIN
RETURN QUERY
    WITH RECURSIVE genre_tree AS (
        -- Base case: start with the given genre
        SELECT 
            0 as level_num,
            g."Id" as genre_id,
            g."Name" as genre_name,
            g."ParentGenreId" as parent_id
        FROM "Genres" g
        WHERE g."Id" = input_genre_id
        
        UNION ALL
        
        -- Recursive case: find children
        SELECT 
            gt.level_num + 1,
            g."Id",
            g."Name",
            g."ParentGenreId"
        FROM "Genres" g
        INNER JOIN genre_tree gt ON g."ParentGenreId" = gt.genre_id
        WHERE g."IsActive" = true
    )
SELECT * FROM genre_tree ORDER BY level_num, genre_name;
END;
$ LANGUAGE plpgsql;

-- =============================================================================
-- SCRIPT COMPLETION MESSAGE
-- =============================================================================

-- Display completion message
DO $block$
BEGIN
    RAISE NOTICE '=== MusicTree Database Initialization Completed Successfully! ===';
END $block$;
