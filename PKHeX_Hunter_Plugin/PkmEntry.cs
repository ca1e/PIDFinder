namespace PKHeX_Hunter_Plugin
{
    public record PkmEntry
    {
        public ulong EC { get; init; }
        public uint PID { get; init; }
        public int ShinyStatus { get; init; }
        public uint Ability { get; init; }
        public uint Nature { get; init; }
        public uint Height { get; init; }
        public uint Weight { get; init; }
        public uint[] ivs { get; init; }

        public uint HP => ivs[0];
        public uint Atk => ivs[1];
        public uint Def => ivs[2];
        public uint SpA => ivs[3];
        public uint SpD => ivs[4];
        public uint Spe => ivs[5];
    }
}
