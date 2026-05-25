-- Add missing SourceFile entry for the ChatGPT source

INSERT INTO SourceFiles (
    Id,
    SourceId,
    RelativePath,
    FileRole,
    CreatedUtc
)
SELECT 
    lower(hex(randomblob(16))),
    Id,
    OriginalRelativePath,
    'original',
    datetime('now')
FROM Sources
WHERE SourceKey = 'chatgpt-manhood-device-dev'
AND NOT EXISTS (
    SELECT 1 FROM SourceFiles sf WHERE sf.SourceId = Sources.Id
);

-- Verify
SELECT s.SourceKey, s.Title, sf.RelativePath, sf.FileRole
FROM Sources s
LEFT JOIN SourceFiles sf ON s.Id = sf.SourceId
WHERE s.SourceKey = 'chatgpt-manhood-device-dev';
