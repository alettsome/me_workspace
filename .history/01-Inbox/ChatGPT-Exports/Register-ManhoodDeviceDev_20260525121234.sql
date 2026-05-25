-- Register ChatGPT export as a Source
-- File: Manhood Device Development.pdf
-- Type: conversation-log (ChatGPT export)

INSERT INTO Sources (
    Id, 
    SourceKey, 
    Title, 
    SourceType, 
    RightsLabel, 
    OriginalRelativePath, 
    CurrentStage, 
    Status, 
    CreatedUtc, 
    UpdatedUtc
) VALUES (
    lower(hex(randomblob(16))),
    'chatgpt-manhood-device-dev',
    'ChatGPT: Manhood Device Development',
    'conversation-log',
    'user-created',
    '01-Inbox/ChatGPT-Exports/Manhood Device Development.pdf',
    'Inbox',
    'new',
    datetime('now'),
    datetime('now')
);

-- Verify insertion
SELECT Id, Title, SourceType, Status, CreatedUtc 
FROM Sources 
WHERE SourceKey = 'chatgpt-manhood-device-dev';
