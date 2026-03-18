using WhereCountRefactorer;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: WhereCountRefactorer <path-to-codebase>");
    return 1;
}

var path = args[0];

Console.WriteLine($"Analyzing: {path}");
Console.WriteLine();

try
{
    // Console.WriteLine("Pattern: .Where(predicate).Count()  →  .Count(predicate)");
    // int refactored = Refactorer.RefactorDirectory(path);
    // Console.WriteLine($"Done. Total replacements: {refactored}");
    // Console.WriteLine();

    Console.WriteLine("Pattern: IQueryable<T>.Any()  →  should have predicate");
    int detected = QueryableAnyDetector.DetectDirectory(path);
    Console.WriteLine($"Done. Total detections: {detected}");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}
