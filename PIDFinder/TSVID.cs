namespace PIDFinder
{
    public record TSVID
    {
        uint _fid
        {
            get
            {
                return SID * 1000000 + TID;
            }
        }
        public uint SID { get; init; }
        public uint TID { get; init; }

        public uint NTID
        {
            get
            {
                return _fid % 65536;
            }
        }

        public uint NSID
        {
            get
            {
                return _fid / 65536;
            }
        }
    }
}
