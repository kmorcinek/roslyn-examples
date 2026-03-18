using WhereCountRefactorer;
using Xunit;

namespace WhereCountRefactorer.Tests;

public class IQueryableAnyDetectorTests
{
    [Fact]
    public void Detects_Any_OnIQueryableParameter()
    {
        var source = """
            using System.Linq;

            class Repo
            {
                void Work(IQueryable<int> query)
                {
                    if (!query.Any()) return;
                }
            }
            """;

        Assert.Equal(1, IQueryableAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Detects_Any_OnIQueryableVariable()
    {
        var source = """
            using System.Linq;

            class Repo
            {
                void Work(IQueryable<int> query)
                {
                    var filtered = query.Where(x => x > 1);
                    if (!filtered.Any()) return;
                }
            }
            """;

        Assert.Equal(1, IQueryableAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_Any_OnList()
    {
        var source = """
            using System.Linq;
            using System.Collections.Generic;

            class Repo
            {
                void Work()
                {
                    var list = new List<int> { 1, 2, 3 };
                    if (!list.Any()) return;
                }
            }
            """;

        Assert.Equal(0, IQueryableAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_AnyWithPredicate()
    {
        var source = """
            using System.Linq;

            class Repo
            {
                void Work(IQueryable<int> query)
                {
                    if (!query.Any(x => x > 0)) return;
                }
            }
            """;

        Assert.Equal(0, IQueryableAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Detects_Multiple_Occurrences()
    {
        var source = """
            using System.Linq;

            class Repo
            {
                void A(IQueryable<int> q) { if (!q.Any()) return; }
                void B(IQueryable<string> q) { if (!q.Any()) return; }
            }
            """;

        Assert.Equal(2, IQueryableAnyDetector.DetectSource(source));
    }
}
