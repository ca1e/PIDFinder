var gd = typeof(PKHeX.Core.GameDataSource);
Console.WriteLine($"== {gd.FullName} ==");
foreach (var p in gd.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))
    Console.WriteLine($"  {(p.GetMethod.IsStatic ? "static " : "")}{p.PropertyType.Name} {p.Name}");
var gv = typeof(PKHeX.Core.GameVersion);
Console.WriteLine($"== GameVersion={gv.FullName} ==");
Console.WriteLine($"  字段示例: {string.Join(", ", Enum.GetNames(gv).Take(5))}");
