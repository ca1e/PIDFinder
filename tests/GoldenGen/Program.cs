using PKHeX.Core;
using PIDFinder.Lib;

var tr = new TrainerIDImpl(12345, 54321);
var rules = new CheckRule { Ability = -1, Gender = -1 };
uint[] seeds = { 0x1234ABCDu, 0x00001234u, 0xDEADBEEFu, 0x0FA0CADEu };
var o = new System.Text.StringBuilder();
o.Append("{\"gen3\":[");
bool first = true;
foreach (var s in seeds)
{
    PKM pk = new PK8();
    if (!PIDFinder.Lib.Method1RNG.TryApplyFromSeed(ref pk, tr, rules, s)) continue;
    if (!first) o.Append(',');
    first = false;
    o.Append($"{{\"seed\":\"{s:X8}\",\"ec\":\"{pk.EncryptionConstant:X8}\",\"pid\":\"{pk.PID:X8}\",\"ivs\":[{pk.IV_HP},{pk.IV_ATK},{pk.IV_DEF},{pk.IV_SPA},{pk.IV_SPD},{pk.IV_SPE}],\"nature\":{pk.Nature},\"ability\":{pk.AbilityNumber},\"shiny\":{(pk.IsShiny ? "true" : "false")}}}");
}
o.Append("],\"overworld8\":[");
first = true;
foreach (var s in seeds)
{
    PKM pk = new PK8();
    if (!PIDFinder.Lib.Overworld8RNG.TryApplyFromSeed(ref pk, tr, rules, s)) continue;
    if (!first) o.Append(',');
    first = false;
    o.Append($"{{\"seed\":\"{s:X8}\",\"ec\":\"{pk.EncryptionConstant:X8}\",\"pid\":\"{pk.PID:X8}\",\"ivs\":[{pk.IV_HP},{pk.IV_ATK},{pk.IV_DEF},{pk.IV_SPA},{pk.IV_SPD},{pk.IV_SPE}],\"nature\":{pk.Nature},\"ability\":{pk.AbilityNumber},\"shiny\":{(pk.IsShiny ? "true" : "false")},\"height\":{((PKHeX.Core.IScaledSize)pk).HeightScalar},\"weight\":{((PKHeX.Core.IScaledSize)pk).WeightScalar}}}");
}
o.Append("],\"roaming8b\":[");
first = true;
foreach (var s in seeds)
{
    PKM pk = new PK8();
    if (!PIDFinder.Lib.Roaming8bRNG.TryApplyFromSeed(ref pk, tr, rules, s)) continue;
    if (!first) o.Append(',');
    first = false;
    o.Append($"{{\"seed\":\"{s:X8}\",\"ec\":\"{pk.EncryptionConstant:X8}\",\"pid\":\"{pk.PID:X8}\",\"ivs\":[{pk.IV_HP},{pk.IV_ATK},{pk.IV_DEF},{pk.IV_SPA},{pk.IV_SPD},{pk.IV_SPE}],\"nature\":{pk.Nature},\"ability\":{pk.AbilityNumber},\"shiny\":{(pk.IsShiny ? "true" : "false")},\"height\":{((PKHeX.Core.IScaledSize)pk).HeightScalar},\"weight\":{((PKHeX.Core.IScaledSize)pk).WeightScalar}}}");
}
o.Append("]}");
File.WriteAllText("/home/agent/playground/PIDFinder/tests/golden.json", o.ToString());
Console.WriteLine("golden saved");

class TrainerIDImpl : PIDFinder.Lib.ITrainerID16
{
    public ushort TID16 { get; set; }
    public ushort SID16 { get; set; }
    public TrainerIDImpl(ushort t, ushort s) { TID16 = t; SID16 = s; }
}
