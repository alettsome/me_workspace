-- Phase 6 Database Migration - Add New Tables
-- This script adds new tables for project management while preserving existing data

-- Create Projects table
CREATE TABLE IF NOT EXISTS "Projects" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "ContentType" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Timeline" TEXT NULL,
    "Description" TEXT NULL,
    "FolderPath" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "UpdatedUtc" TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Projects_Name" ON "Projects" ("Name");
CREATE INDEX IF NOT EXISTS "IX_Projects_Status" ON "Projects" ("Status");

-- Create Summaries table
CREATE TABLE IF NOT EXISTS "Summaries" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "SourceId" TEXT NOT NULL,
    "ProjectId" TEXT NOT NULL,
    "SummaryText" TEXT NOT NULL,
    "PageRange" TEXT NULL,
    "Keywords" TEXT NULL,
    "EmbeddingVector" BLOB NULL,
    "EmbeddingModel" TEXT NULL,
    "RelevanceScore" REAL NULL,
    "CreatedUtc" TEXT NOT NULL,
    FOREIGN KEY ("SourceId") REFERENCES "Sources"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Summaries_ProjectId" ON "Summaries" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_Summaries_SourceId" ON "Summaries" ("SourceId");

-- Create DocumentAnchors table
CREATE TABLE IF NOT EXISTS "DocumentAnchors" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "ProjectId" TEXT NOT NULL,
    "AnchorName" TEXT NOT NULL,
    "AnchorKey" TEXT NOT NULL,
    "Position" INTEGER NOT NULL,
    "Status" TEXT NOT NULL,
    "CompletionPercent" INTEGER NOT NULL DEFAULT 0,
    "Description" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "UpdatedUtc" TEXT NOT NULL,
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_DocumentAnchors_ProjectId_Position" ON "DocumentAnchors" ("ProjectId", "Position");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DocumentAnchors_ProjectId_AnchorKey" ON "DocumentAnchors" ("ProjectId", "AnchorKey");

-- Create ProjectTasks table
CREATE TABLE IF NOT EXISTS "ProjectTasks" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "ProjectId" TEXT NOT NULL,
    "AnchorId" TEXT NULL,
    "ConversationId" TEXT NULL,
    "Description" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Priority" TEXT NULL,
    "DueDate" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "UpdatedUtc" TEXT NOT NULL,
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("AnchorId") REFERENCES "DocumentAnchors"("Id") ON DELETE SET NULL,
    FOREIGN KEY ("ConversationId") REFERENCES "Conversations"("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_ProjectTasks_ProjectId" ON "ProjectTasks" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_ProjectTasks_Status" ON "ProjectTasks" ("Status");

-- Create AgentLogs table
CREATE TABLE IF NOT EXISTS "AgentLogs" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "ProjectId" TEXT NOT NULL,
    "AgentType" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "Input" TEXT NULL,
    "Result" TEXT NULL,
    "Status" TEXT NOT NULL,
    "ErrorMessage" TEXT NULL,
    "DurationMs" INTEGER NULL,
    "Metadata" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    "CompletedUtc" TEXT NULL,
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AgentLogs_ProjectId" ON "AgentLogs" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_AgentLogs_CreatedUtc" ON "AgentLogs" ("CreatedUtc");

-- Create TrendAnalyses table
CREATE TABLE IF NOT EXISTS "TrendAnalyses" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "ProjectId" TEXT NOT NULL,
    "Topic" TEXT NOT NULL,
    "TrendSummary" TEXT NOT NULL,
    "SourceCount" INTEGER NOT NULL,
    "PrevalencePercent" REAL NULL,
    "SourceIds" TEXT NULL,
    "Evidence" TEXT NULL,
    "Recommendations" TEXT NULL,
    "CreatedUtc" TEXT NOT NULL,
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_TrendAnalyses_ProjectId" ON "TrendAnalyses" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_TrendAnalyses_Topic" ON "TrendAnalyses" ("Topic");
