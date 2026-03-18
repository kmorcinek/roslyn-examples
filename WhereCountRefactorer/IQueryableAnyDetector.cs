using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhereCountRefactorer;

/// <summary>
/// Detects .Any() (no predicate) calls where the receiver is IQueryable&lt;T&gt;.
/// Uses the semantic model to resolve types, so it catches variable-based patterns
/// that pure syntactic analysis (WhereAnyDetector) cannot see.
/// </summary>
public class IQueryableAnyDetector : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private readonly SyntaxTree _tree;
    private readonly SemanticModel _semanticModel;

    public int DetectionCount { get; private set; }

    private IQueryableAnyDetector(string filePath, SyntaxTree tree, SemanticModel semanticModel)
    {
        _filePath = filePath;
        _tree = tree;
        _semanticModel = semanticModel;
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

        // Use the semantic model to check the receiver's type
        var receiverType = _semanticModel.GetTypeInfo(anyAccess.Expression).Type;
        if (receiverType == null)
            return;

        if (!IsIQueryable(receiverType))
            return;

        var lineSpan = _tree.GetLineSpan(node.Span);
        var line = lineSpan.StartLinePosition.Line + 1;

        Console.WriteLine($"  [iqueryable-any] {_filePath}:{line}  →  IQueryable<T>.Any()");
        DetectionCount++;
    }

    private static bool IsIQueryable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            if (IsIQueryableNamed(namedType))
                return true;

            return namedType.AllInterfaces.Any(IsIQueryableNamed);
        }
        return false;
    }

    private static bool IsIQueryableNamed(INamedTypeSymbol type) =>
        type.OriginalDefinition.Name == "IQueryable"
        && type.ContainingNamespace?.ToString() == "System.Linq";

    /// <summary>Detects occurrences in a source string. Exposed for unit testing.</summary>
    public static int DetectSource(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("test",
            [tree],
            GetPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(tree);
        var detector = new IQueryableAnyDetector("<test>", tree, semanticModel);
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

        var references = GetPlatformReferences().ToArray();
        int total = 0;

        foreach (var file in csFiles)
        {
            var source = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(file),
                [tree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var semanticModel = compilation.GetSemanticModel(tree);
            var detector = new IQueryableAnyDetector(file, tree, semanticModel);
            detector.Visit(tree.GetRoot());
            total += detector.DetectionCount;
        }

        return total;
    }

    private static IEnumerable<MetadataReference> GetPlatformReferences()
    {
        var trustedPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator);
        return trustedPaths.Select(p => MetadataReference.CreateFromFile(p));
    }
}
