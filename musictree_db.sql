-- MusicTree Database Initialization Script
-- Project: MusicTree para X-Tec
-- Sprint 1 Implementation - Backend & Database
-- Author: Katharsis, AudioSignal Labs.
-- Date: May 2025

-- =============================================================================
-- DATABASE AND USER SETUP
-- =============================================================================

-- Create database (run as postgres superuser)
-- CREATE DATABASE musictree_db;

-- Create application user
-- CREATE USER musictree_admin WITH PASSWORD 'musictree';
-- GRANT ALL PRIVILEGES ON DATABASE musictree TO musictree_admin;

-- Connect to musictree database and run the following as musictree_admin
-- \c musictree;

-- =============================================================================
-- SCHEMA CREATION
-- =============================================================================

-- Drop tables if they exist (for clean reinstallation)
DROP TABLE IF EXISTS "GenreRelations" CASCADE;
DROP TABLE IF EXISTS "Genres" CASCADE;
DROP TABLE IF EXISTS "Clusters" CASCADE;



-- Add as needed


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

-- Set table owner
ALTER TABLE "Clusters" OWNER TO musictree_admin;

-- Add table comment
COMMENT ON TABLE "Clusters" IS 'Music genre clusters for high-level categorization';
COMMENT ON COLUMN "Clusters"."Id" IS 'Unique cluster identifier';
COMMENT ON COLUMN "Clusters"."Name" IS 'Display name of the cluster';
COMMENT ON COLUMN "Clusters"."Description" IS 'Detailed description of the cluster';
COMMENT ON COLUMN "Clusters"."IsActive" IS 'Indicates if cluster is currently active';
COMMENT ON COLUMN "Clusters"."TimeStamp" IS 'Record creation/modification timestamp';

-- =============================================================================
-- TABLE: Genres
-- Description: Music genres with detailed characteristics
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
    "Color"              VARCHAR(7),
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
    CONSTRAINT "CK_Genres_Color" 
        CHECK ("Color" IS NULL OR "Color" ~ '^#[0-9A-Fa-f]{6}$'),
    
    -- Unique Constraints
    CONSTRAINT "UQ_Genres_Name" UNIQUE ("Name")
);

-- Set table owner
ALTER TABLE "Genres" OWNER TO musictree_admin;

-- Add table comments
COMMENT ON TABLE "Genres" IS 'Music genres with detailed musical characteristics';
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
COMMENT ON COLUMN "Genres"."Color" IS 'Hex color code for UI representation';
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

-- Set table owner
ALTER TABLE "GenreRelations" OWNER TO musictree_admin;

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

-- Genres indexes
CREATE INDEX "IX_Genres_Name" ON "Genres"("Name");
CREATE INDEX "IX_Genres_IsSubgenre" ON "Genres"("IsSubgenre");
CREATE INDEX "IX_Genres_ParentGenreId" ON "Genres"("ParentGenreId");
CREATE INDEX "IX_Genres_ClusterId" ON "Genres"("ClusterId");
CREATE INDEX "IX_Genres_IsActive" ON "Genres"("IsActive");
CREATE INDEX "IX_Genres_BpmRange" ON "Genres"("BpmLower", "BpmUpper");
CREATE INDEX "IX_Genres_CreationYear" ON "Genres"("GenreCreationYear");
CREATE INDEX "IX_Genres_OriginCountry" ON "Genres"("GenreOriginCountry");

-- GenreRelations indexes
CREATE INDEX "IX_GenreRelations_RelatedGenreId" ON "GenreRelations"("RelatedGenreId");
CREATE INDEX "IX_GenreRelations_Influence" ON "GenreRelations"("Influence");
CREATE INDEX "IX_GenreRelations_MGPC" ON "GenreRelations"("MGPC");

-- =============================================================================
-- SAMPLE DATA 
-- =============================================================================

-- Insert sample clusters (IDS no siguen convención, es únicamente para test)
INSERT INTO "Clusters" ("Id", "Name", "Description", "IsActive") VALUES
('cluster-electronic', 'Electronic', 'Electronic music and derivatives', true),
('cluster-rock', 'Rock', 'Rock music and its subgenres', true),
('cluster-pop', 'Pop', 'Popular music genres', true),
('cluster-classical', 'Classical', 'Classical and orchestral music', true),
('cluster-world', 'World', 'Traditional and world music', true);

-- Insert sample genres
INSERT INTO "Genres" ("Id", "Name", "Description", "IsSubgenre", "ParentGenreId", "ClusterId", 
                     "Key", "BpmLower", "BpmUpper", "Bpm", "Color", "GenreCreationYear", 
                     "GenreOriginCountry", "GenreTipicalMode", "Volume", "CompasMetric", "AvrgDuration") VALUES
('genre-rock', 'Rock', 'Traditional rock music', false, null, 'cluster-rock', 
 -1, 120, 140, 130, '#FF6B35', 1950, 'United States', 0.7, -15, 4, 240),
('genre-pop', 'Pop', 'Popular mainstream music', false, null, 'cluster-pop', 
 -1, 100, 130, 115, '#FF1493', 1950, 'United States', 0.8, -12, 4, 210),
('genre-electronic', 'Electronic', 'Electronic dance music', false, null, 'cluster-electronic', 
 -1, 120, 150, 128, '#00FFFF', 1970, 'Germany', 0.6, -18, 4, 300),
('genre-classical', 'Classical', 'Classical orchestral music', false, null, 'cluster-classical', 
 -1, 60, 120, 90, '#8B4513', 1600, 'Europe', 0.5, -25, 4, 420);

-- Insert sample genre relations
INSERT INTO "GenreRelations" ("GenreId", "RelatedGenreId", "Influence", "MGPC") VALUES
('genre-rock', 'genre-pop', 7, 0.6),
('genre-pop', 'genre-rock', 5, 0.4),
('genre-electronic', 'genre-pop', 6, 0.5),
('genre-classical', 'genre-rock', 3, 0.2);

-- =============================================================================
-- VIEWS 
-- =============================================================================

-- View for active genres with cluster information
CREATE VIEW "ActiveGenresWithClusters" AS
SELECT 
    g."Id",
    g."Name",
    g."Description",
    g."IsSubgenre",
    pg."Name" AS "ParentGenreName",
    c."Name" AS "ClusterName",
    g."Bpm",
    g."Color",
    g."GenreCreationYear",
    g."GenreOriginCountry"
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
-- FUNCTIONS 
-- =============================================================================

-- Function to get genre hierarchy
CREATE OR REPLACE FUNCTION GetGenreHierarchy(genre_id TEXT)
RETURNS TABLE(
    level_num INTEGER,
    genre_id TEXT,
    genre_name VARCHAR(30),
    parent_id TEXT
) AS $$
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
        WHERE g."Id" = GetGenreHierarchy.genre_id
        
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
$$ LANGUAGE plpgsql;

-- =============================================================================
-- GRANTS AND PERMISSIONS
-- =============================================================================

-- Grant permissions to application user
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO musictree_admin;
GRANT SELECT ON ALL VIEWS IN SCHEMA public TO musictree_admin;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO musictree_admin;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO musictree_admin;

-- =============================================================================
-- SCRIPT COMPLETION
-- =============================================================================

-- Display completion message
DO $$
BEGIN
    RAISE NOTICE 'MusicTree database initialization completed successfully!';
    RAISE NOTICE 'Tables created: Clusters, Genres, GenreRelations';
    RAISE NOTICE 'Views created: ActiveGenresWithClusters, GenreRelationshipsWithNames';
    RAISE NOTICE 'Functions created: GetGenreHierarchy';
    RAISE NOTICE 'Sample data inserted for testing purposes';
END $$;
