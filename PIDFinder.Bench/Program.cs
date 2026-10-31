using System.Diagnostics;
using PIDFinder.Lib;

if (args.Length > 1 || (args.Length > 0 && (!uint.TryParse(args[0], out uint requestedCount) || requestedCount == 0)))
{
    Console.Error.WriteLine("Usage: dotnet run -c Release --project PIDFinder.Bench -- [positive seed-count]");
    return 1;
}
uint count = args.Length == 0 ? 100_000u : uint.Parse(args[0]);
var rules = new CheckRule { Ability = -1, Gender = -1, Shiny = ShinyType.Shiny };
Console.WriteLine($"Scan {count:N0} numeric seeds per run; retain at most 1000 shiny matches; {Environment.ProcessorCount} logical CPUs.");
Console.WriteLine("Stopwatch timings include generation, filtering, collection and sorting; not a statistical benchmark.");
foreach (var kind in Enum.GetValues<RngKind>())
{
    // Warm up both execution paths before measuring.
    RngScanner.Scan(kind, 0x10000000, Math.Min(count, 4096u), 12345, 54321, rules, false, 1024, maxResults: 1000);
    RngScanner.Scan(kind, 0x10000000, Math.Min(count, 4096u), 12345, 54321, rules, true, 1024, maxResults: 1000);
    uint[] serialSeeds = Array.Empty<uint>();
    foreach (bool parallel in new[] { false, true })
    {
        var timer = Stopwatch.StartNew();
        var results = RngScanner.Scan(kind, 0x10000000, count, 12345, 54321, rules,
            parallel, chunkSize: 4096, maxResults: 1000);
        timer.Stop();
        var seeds = results.Select(r => r.Seed).ToArray();
        if (!parallel) serialSeeds = seeds;
        else if (!serialSeeds!.SequenceEqual(seeds))
            throw new InvalidOperationException($"{kind}: parallel and serial results differ.");
        Console.WriteLine($"{kind,-12} {(parallel ? "parallel" : "serial"),-8}: {timer.Elapsed.TotalMilliseconds,10:F2} ms; matches={results.Count}");
    }
}
return 0;
