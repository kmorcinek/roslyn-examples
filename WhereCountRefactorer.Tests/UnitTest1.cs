using WhereCountRefactorer;
using Xunit;

namespace WhereCountRefactorer.Tests;

public class WhereCountRewriterTests
{
    [Fact]
    public void Rewrites_WhereCount_To_Count_With_Predicate()
    {
        var source = """
            using System.Linq;
            using System.Collections.Generic;

            var list = new List<int> { 1, 2, 3 };
            var result = list.Where(x => x > 1).Count();
            """;

        var expected = """
            using System.Linq;
            using System.Collections.Generic;

            var list = new List<int> { 1, 2, 3 };
            var result = list.Count(x => x > 1);
            """;

        var (newSource, count) = Refactorer.RefactorSource(source);

        Assert.Equal(1, count);
        Assert.Equal(expected, newSource);
    }

    [Fact]
    public void Does_Not_Rewrite_Count_Without_Where()
    {
        var source = """
            using System.Linq;
            using System.Collections.Generic;

            var list = new List<int> { 1, 2, 3 };
            var result = list.Count();
            """;

        var (newSource, count) = Refactorer.RefactorSource(source);

        Assert.Equal(0, count);
        Assert.Equal(source, newSource);
    }
}
