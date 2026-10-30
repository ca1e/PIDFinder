namespace PIDFinder.Lib
{
    /// <summary>由种子推导出的宝可梦生成结果（纯数据，与 UI / 存档类型解耦）。</summary>
    public sealed class PkmResult
    {
        public uint Seed { get; set; }
        public uint EC { get; set; }
        public uint PID { get; set; }
        public int ShinyXor { get; set; }
        public int[] IVs { get; set; } = new int[6];
        public int Nature { get; set; }
        public int Ability { get; set; }
        public byte HeightScalar { get; set; }
        public byte WeightScalar { get; set; }
    }
}
