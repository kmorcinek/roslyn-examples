using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhereCountRefactorer;

/// <summary>
/// Purely syntactic detector. Finds variables declared as:
///   var x = *.Where(...)
/// and reports any subsequent x.Any() calls on them.
/// Works without a semantic model, so it catches patterns where the source
/// type (e.g. EF's DbSet&lt;T&gt;) is unavailable at analysis time.
/// </summary>
public class WhereVarAnyDetector : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private readonly SyntaxTree _tree;
    private readonly HashSet<string> _whereVarNames = new();

    public int DetectionCount { get; private set; }

    private WhereVarAnyDetector(string filePath, SyntaxTree tree)
    {
        _filePath = filePath;
        _tree = tree;
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        base.VisitLocalDeclarationStatement(node);

        // Only interested in `var` declarations
        if (!node.Declaration.Type.IsVar)
            return;

        foreach (var variable in node.Declaration.Variables)
        {
            if (variable.Initializer?.Value is InvocationExpressionSyntax initCall
                && IsWhereCall(initCall))
            {
                _whereVarNames.Add(variable.Identifier.Text);
            }
        }
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        base.VisitInvocationExpression(node);

        if (node.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (!memberAccess.Name.Identifier.Text.Equals("Any", StringComparison.Ordinal))
            return;

        if (node.ArgumentList.Arguments.Count != 0)
            return;

        if (memberAccess.Expression is not IdentifierNameSyntax identifier)
            return;

        if (!_whereVarNames.Contains(identifier.Identifier.Text))
            return;

        var lineSpan = _tree.GetLineSpan(node.Span);
        var line = lineSpan.StartLinePosition.Line + 1;

        Console.WriteLine($"  [where-var-any] {_filePath}:{line}  →  {identifier.Identifier.Text}.Any()");
        DetectionCount++;
    }

    private static bool IsWhereCall(InvocationExpressionSyntax call) =>
        call.Expression is MemberAccessExpressionSyntax access
        && access.Name.Identifier.Text.Equals("Where", StringComparison.Ordinal)
        && call.ArgumentList.Arguments.Count == 1;

    public static int DetectSource(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var detector = new WhereVarAnyDetector("<test>", tree);
        detector.Visit(tree.GetRoot());
        return detector.DetectionCount;
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
            var detector = new WhereVarAnyDetector(file, tree);
            detector.Visit(tree.GetRoot());
            total += detector.DetectionCount;
        }

        return total;
    }
}
