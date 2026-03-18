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

    // Console.WriteLine("Pattern: .Where(predicate).Any()  →  .Any(predicate)");
    // int detected = WhereAnyDetector.DetectDirectory(path);
    // Console.WriteLine($"Done. Total detections: {detected}");

    Console.WriteLine("Pattern: IQueryable<T>.Any()  →  should have predicate");
    int detected = QueryableAnyDetector.DetectDirectory(path);
    Console.WriteLine($"Done. Total detections: {detected}");
    Console.WriteLine();

    Console.WriteLine("Pattern: var x = *.Where(...)  +  x.Any()  →  should have predicate");
    int detected2 = WhereVarAnyDetector.DetectDirectory(path);
    Console.WriteLine($"Done. Total detections: {detected2}");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 2;
}
