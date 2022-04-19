namespace PKHeX_Roaming8b_Plugin
{
    public record PkmEntry
    {
        public ulong EC { get; set; }
        public uint PID { get; set; }
        public int ShinyStatus { get; set; }
        public uint Ability { get; set; }
        public uint Nature { get; set; }
        public uint Height { get; set; }
        public uint Weight { get; set; }
        public uint[] ivs { get; set; }

        public uint HP => ivs[0];
        public uint Atk => ivs[1];
        public uint Def => ivs[2];
        public uint SpA => ivs[3];
        public uint SpD => ivs[4];
        public uint Spe => ivs[5];
    }
}
