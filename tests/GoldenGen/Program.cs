using System.Text.Json;
using PIDFinder.Lib;

// Development fixture exporter. Generated output is not an independent oracle:
// review against PKHeX/reference fixtures before replacing golden expectations.
var rules = new CheckRule { Ability = -1, Gender = -1 };
uint[] seeds = { 0x1234ABCDu, 0x00001234u, 0xDEADBEEFu, 0x0FA0CADEu };
object[] Generate(RngKind kind) => seeds.Select(seed =>
{
    var result = RngScanner.Search(kind, seed, 1, 12345, 54321, rules)
        ?? throw new InvalidOperationException($"{kind}: seed {seed:X8} did not generate a result.");
    return (object)new
    {
        seed = seed.ToString("X8"),
        ec = result.EC.ToString("X8"),
        pid = result.PID.ToString("X8"),
        ivs = result.IVs,
        nature = result.Nature,
        // Preserve historical fixture encoding: ordinary slots use 1/2; unknown uses 0.
        ability = result.Ability < 0 ? 0 : result.Ability + 1,
        shiny = result.ShinyXor < (kind == RngKind.Method1 ? 8 : 16),
        height = result.HeightScalar,
        weight = result.WeightScalar,
    };
}).ToArray();

string DefaultOutputPath()
{
    for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "PIDFinder.sln")))
            return Path.Combine(directory.FullName, "tests", "golden.json");
    }
    throw new InvalidOperationException("Cannot locate PIDFinder.sln; pass an explicit output JSON path.");
}

if (args.Length > 1)
    throw new ArgumentException("Usage: GoldenGen [output-json-path]");
string output = args.Length == 1 ? Path.GetFullPath(args[0]) : DefaultOutputPath();
var document = new Dictionary<string, object[]>
{
    ["gen3"] = Generate(RngKind.Method1),
    ["overworld8"] = Generate(RngKind.Overworld8),
    ["roaming8b"] = Generate(RngKind.Roaming8b),
};
File.WriteAllText(output, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Raw correlation fixtures saved to {output}. Review independently before accepting as golden data.");
