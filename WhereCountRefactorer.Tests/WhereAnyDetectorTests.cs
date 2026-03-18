using WhereCountRefactorer;
using Xunit;

namespace WhereCountRefactorer.Tests;

public class WhereAnyDetectorTests
{
    [Fact]
    public void Detects_DirectChain_WhereAny()
    {
        var source = """
            var list = new List<int> { 1, 2, 3 };
            var result = list.Where(x => x > 1).Any();
            """;

        Assert.Equal(1, WhereAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_AnyWithPredicate()
    {
        var source = """
            var list = new List<int> { 1, 2, 3 };
            var result = list.Any(x => x > 1);
            """;

        Assert.Equal(0, WhereAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_AnyWithoutWhere()
    {
        var source = """
            var list = new List<int> { 1, 2, 3 };
            var result = list.Any();
            """;

        Assert.Equal(0, WhereAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Does_Not_Detect_VariableIntermediary()
    {
        // The Where result is stored in a variable — not a direct syntactic chain,
        // so the detector cannot catch it without data-flow analysis.
        var source = """
            var list = new List<int> { 1, 2, 3 };
            var filtered = list.Where(x => x > 1);
            var result = filtered.Any();
            """;

        Assert.Equal(0, WhereAnyDetector.DetectSource(source));
    }

    [Fact]
    public void Detects_Multiple_Occurrences()
    {
        var source = """
            var list = new List<int> { 1, 2, 3 };
            var a = list.Where(x => x > 1).Any();
            var b = list.Where(x => x < 0).Any();
            """;

        Assert.Equal(2, WhereAnyDetector.DetectSource(source));
    }
}
