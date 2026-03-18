using WhereCountRefactorer;
using Xunit;

namespace WhereCountRefactorer.Tests;

public class WhereVarAnyDetectorTests
{
    [Fact]
    public void Detects_VarAssignedFromWhere_ThenAny()
    {
        var source = """
            var entries = dbSet.Where(x => x.Active);
            if (!entries.Any()) return;
            """;

        Assert.Equal(1, WhereVarAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_WhenAnyHasPredicate()
    {
        var source = """
            var entries = dbSet.Where(x => x.Active);
            if (!entries.Any(x => x.Id > 0)) return;
            """;

        Assert.Equal(0, WhereVarAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_WhenVarNotFromWhere()
    {
        var source = """
            var entries = dbSet.ToList();
            if (!entries.Any()) return;
            """;

        Assert.Equal(0, WhereVarAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_DirectChain_WhereAny()
    {
        // Direct chain is WhereAnyDetector's job, not this one
        var source = """
            var result = dbSet.Where(x => x.Active).Any();
            """;

        Assert.Equal(0, WhereVarAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Detects_CacheRepository_Pattern()
    {
        var source = """
            var expiredDate = DateTime.UtcNow.Add(-timespan);
            var expiredEntries = DbSet.Where(x => x.Created < expiredDate);
            if (!expiredEntries.Any())
            {
                return;
            }
            """;

        Assert.Equal(1, WhereVarAnyDetector.DetectSource(source));
    }
}
