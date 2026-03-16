using WhereCountRefactorer;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: WhereCountRefactorer <path-to-codebase>");
    return 1;
}

var path = args[0];

Console.WriteLine($"Refactoring: {path}");
Console.WriteLine("Pattern: .Where(predicate).Count()  →  .Count(predicate)");
Console.WriteLine();

try
{
    int total = Refactorer.RefactorDirectory(path);
    Console.WriteLine();
    Console.WriteLine($"Done. Total replacements: {total}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}
