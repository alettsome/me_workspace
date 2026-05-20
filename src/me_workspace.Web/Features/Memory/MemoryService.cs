using Microsoft.EntityFrameworkCore;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;

namespace me_workspace.Web.Features.Memory;

public sealed class MemoryService(AppDbContext db)
{
    public async Task<IReadOnlyList<MemoryItemDto>> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        return await db.MemoryItems
            .AsNoTracking()
            .OrderByDescending(x => x.Pinned)
            .ThenBy(x => x.Key)
            .Select(item => new MemoryItemDto(item.Id, item.Key, item.Content, item.Pinned, item.CreatedUtc, item.UpdatedUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemoryItemDto>> GetPinnedItemsAsync(CancellationToken cancellationToken)
    {
        return await db.MemoryItems
            .AsNoTracking()
            .Where(x => x.Pinned)
            .OrderBy(x => x.Key)
            .Select(item => new MemoryItemDto(item.Id, item.Key, item.Content, item.Pinned, item.CreatedUtc, item.UpdatedUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<MemoryItemDto?> CreateItemAsync(CreateMemoryItemRequest request, CancellationToken cancellationToken)
    {
        var key = request.Key?.Trim();
        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var item = new MemoryItem
        {
            Key = key,
            Content = content,
            Pinned = request.Pinned,
            UpdatedUtc = DateTime.UtcNow
        };

        db.MemoryItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return new MemoryItemDto(item.Id, item.Key, item.Content, item.Pinned, item.CreatedUtc, item.UpdatedUtc);
    }

    public async Task<MemoryItemDto?> UpdateItemAsync(Guid id, UpdateMemoryItemRequest request, CancellationToken cancellationToken)
    {
        var key = request.Key?.Trim();
        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var item = await db.MemoryItems.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.Key = key;
        item.Content = content;
        item.Pinned = request.Pinned;
        item.UpdatedUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new MemoryItemDto(item.Id, item.Key, item.Content, item.Pinned, item.CreatedUtc, item.UpdatedUtc);
    }

    public async Task<bool> DeleteItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.MemoryItems.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.MemoryItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
