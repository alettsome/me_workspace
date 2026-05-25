CREATE TABLE IF NOT EXISTS ProcessingNotifications (
    Id TEXT NOT NULL PRIMARY KEY,
    SourceId TEXT NULL,
    FileName TEXT NOT NULL,
    Status TEXT NOT NULL,
    Message TEXT NULL,
    ChunksCreated INTEGER NULL,
    CharactersProcessed INTEGER NULL,
    ProcessingTimeMs INTEGER NULL,
    CreatedUtc TEXT NOT NULL,
    IsRead INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (SourceId) REFERENCES Sources(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_ProcessingNotifications_SourceId ON ProcessingNotifications(SourceId);
CREATE INDEX IF NOT EXISTS IX_ProcessingNotifications_CreatedUtc ON ProcessingNotifications(CreatedUtc);
CREATE INDEX IF NOT EXISTS IX_ProcessingNotifications_IsRead ON ProcessingNotifications(IsRead);
