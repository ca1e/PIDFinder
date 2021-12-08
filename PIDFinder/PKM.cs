namespace PIDFinder
{
    public record PKM
    {
        public ulong EC { get; init; }
        public uint PID { get; init; }

        public Shiny ShinyStatus { get; init; }

        public string GetShinyString() => ShinyStatus switch
        {
            Shiny.AlwaysSquare => "Square!",
            Shiny.AlwaysStar => "Star!",
            Shiny.Never => "No Shiny",
            _ => "Unknown",
        };

        public uint[] ivs { get; init; }

        public uint HP {
            get
            {
                return ivs[0];
            }
        }
        public uint Atk
        {
            get
            {
                return ivs[1];
            }
        }
        public uint Def
        {
            get
            {
                return ivs[2];
            }
        }
        public uint SpA
        {
            get
            {
                return ivs[3];
            }
        }
        public uint SpD
        {
            get
            {
                return ivs[4];
            }
        }
        public uint Spe
        {
            get
            {
                return ivs[5];
            }
        }

        public uint Ability { get; init; }
        public uint Height { get; init; }
        public uint Weight { get; init; }
    }
}
