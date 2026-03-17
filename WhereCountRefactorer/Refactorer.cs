using Microsoft.CodeAnalysis.CSharp;

namespace WhereCountRefactorer;

public static class Refactorer
{
    /// <summary>
    /// Processes all .cs files in the given directory tree,
    /// rewriting Where(...).Count() → Count(...) in each file.
    /// </summary>
    /// <returns>Total number of replacements made.</returns>
    public static int RefactorDirectory(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");

        var csFiles = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories);
        int total = 0;

        foreach (var file in csFiles)
        {
            var replaced = RefactorFile(file);
            if (replaced > 0)
            {
                Console.WriteLine($"  [{replaced} change(s)] {file}");
                total += replaced;
            }
            else
            {
                Console.WriteLine($"  [no changes]    {file}");
            }
        }

        return total;
    }

    /// <summary>
    /// Processes a single .cs file. Returns the number of replacements made.
    /// </summary>
    public static int RefactorFile(string filePath)
    {
        var source = File.ReadAllText(filePath);
        var (newSource, count) = RefactorSource(source);

        if (count > 0)
            File.WriteAllText(filePath, newSource);

        return count;
    }

    /// <summary>
    /// Applies the rewrite to a source string and returns the new source + replacement count.
    /// Exposed for unit testing without touching the filesystem.
    /// </summary>
    public static (string NewSource, int ReplacementCount) RefactorSource(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        var rewriter = new WhereCountRewriter();
        var newRoot = rewriter.Visit(root);

        return (newRoot.ToFullString(), rewriter.ReplacementCount);
    }
}
