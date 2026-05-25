-- Check GUID formats in the database
SELECT 
    Id,
    SourceKey,
    Title,
    hex(Id) as IdHex,
    length(Id) as IdLength,
    typeof(Id) as IdType
FROM Sources
WHERE SourceKey LIKE '%manhood%';

-- Check SourceFiles for this source
SELECT 
    sf.Id,
    sf.SourceId,
    sf.RelativePath,
    hex(sf.SourceId) as SourceIdHex,
    length(sf.SourceId) as SourceIdLength
FROM SourceFiles sf
WHERE sf.SourceId IN (
    SELECT Id FROM Sources WHERE SourceKey LIKE '%manhood%'
);
