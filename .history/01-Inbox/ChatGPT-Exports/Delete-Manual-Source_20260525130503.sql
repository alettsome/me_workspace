-- Delete manually created source so scan API can recreate it properly
DELETE FROM SourceFiles WHERE SourceId IN (
    SELECT Id FROM Sources WHERE SourceKey = 'chatgpt-manhood-device-dev'
);

DELETE FROM Sources WHERE SourceKey = 'chatgpt-manhood-device-dev';

-- Verify deletion
SELECT COUNT(*) as RemainingCount FROM Sources WHERE SourceKey = 'chatgpt-manhood-device-dev';
