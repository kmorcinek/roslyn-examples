using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhereCountRefactorer;

/// <summary>
/// Detects .Where(predicate).Any() calls on IQueryable — a pattern that causes
/// an extra round-trip: the Where materializes a filtered query, then Any() issues
/// another query. Should be rewritten to a single .Any(predicate) call.
/// </summary>
public class WhereAnyDetector : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private readonly SyntaxTree _tree;

    public int DetectionCount { get; private set; }

    private WhereAnyDetector(string filePath, SyntaxTree tree)
    {
        _filePath = filePath;
        _tree = tree;
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        base.VisitInvocationExpression(node);

        // Match: <expr>.Any()  — no arguments
        if (node.Expression is not MemberAccessExpressionSyntax anyAccess)
            return;

        if (!anyAccess.Name.Identifier.Text.Equals("Any", StringComparison.Ordinal))
            return;

        if (node.ArgumentList.Arguments.Count != 0)
            return;

        // The receiver of .Any() must itself be a .Where(predicate) call
        if (anyAccess.Expression is not InvocationExpressionSyntax whereCall)
            return;

        if (whereCall.Expression is not MemberAccessExpressionSyntax whereAccess)
            return;

        if (!whereAccess.Name.Identifier.Text.Equals("Where", StringComparison.Ordinal))
            return;

        if (whereCall.ArgumentList.Arguments.Count != 1)
            return;

        var lineSpan = _tree.GetLineSpan(node.Span);
        var line = lineSpan.StartLinePosition.Line + 1;

        Console.WriteLine($"  [where-any] {_filePath}:{line}  →  .Where(predicate).Any()");
        DetectionCount++;
    }

    public static int DetectDirectory(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");

        var csFiles = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                     && !f.EndsWith("Reference.cs", StringComparison.OrdinalIgnoreCase)
                     && !f.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)
                     && !f.Split(Path.DirectorySeparatorChar).Any(part => part == "obj" || part == "Migrations" || part.EndsWith("Tests")));

        int total = 0;

        foreach (var file in csFiles)
        {
            var source = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(source);
            var root = tree.GetRoot();

            var detector = new WhereAnyDetector(file, tree);
            detector.Visit(root);
            total += detector.DetectionCount;
        }

        return total;
    }
}
