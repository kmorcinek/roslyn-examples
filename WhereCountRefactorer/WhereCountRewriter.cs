using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WhereCountRefactorer;

/// <summary>
/// Rewrites .Where(predicate).Count() into .Count(predicate)
/// </summary>
public class WhereCountRewriter : CSharpSyntaxRewriter
{
    public int ReplacementCount { get; private set; }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Visit children first (bottom-up)
        node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;

        // Match: <expr>.Count()  — no arguments
        if (node.Expression is not MemberAccessExpressionSyntax countAccess)
            return node;

        if (!countAccess.Name.Identifier.Text.Equals("Count", StringComparison.Ordinal))
            return node;

        if (node.ArgumentList.Arguments.Count != 0)
            return node;

        // The receiver of .Count() must itself be a .Where(predicate) call
        if (countAccess.Expression is not InvocationExpressionSyntax whereCall)
            return node;

        if (whereCall.Expression is not MemberAccessExpressionSyntax whereAccess)
            return node;

        if (!whereAccess.Name.Identifier.Text.Equals("Where", StringComparison.Ordinal))
            return node;

        if (whereCall.ArgumentList.Arguments.Count != 1)
            return node;

        // Build replacement: <originalCollection>.Count(<predicate>)
        var collection = whereAccess.Expression;
        var predicate = whereCall.ArgumentList.Arguments[0];

        var newNode = node
            .WithExpression(
                countAccess.WithExpression(collection))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(predicate)))
            .WithLeadingTrivia(node.GetLeadingTrivia())
            .WithTrailingTrivia(node.GetTrailingTrivia());

        ReplacementCount++;
        return newNode;
    }
}
