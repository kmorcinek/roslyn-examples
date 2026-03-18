using System;
using System.Linq;
using System.Data.Entity;

namespace SampleApp;

public class CacheRepository
{
    private readonly DbSet<CacheEntry> DbSet;

    public void RemoveExpired(TimeSpan timespan)
    {
        var expiredDate = DateTime.UtcNow.Add(-timespan);
        var expiredEntries = DbSet.Where(x => x.Created < expiredDate);
        if (!expiredEntries.Any())
        {
            return;
        }
        foreach (var entry in expiredEntries)
        {
            DbSet.Remove(entry);
        }
    }
}

public class CacheEntry
{
    public DateTime Created { get; set; }
}
