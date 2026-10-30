namespace PIDFinder.Lib
{
    /// <summary>只读训练家 ID（Gen3/Gen8 通用 16 位 ID）。</summary>
    public interface ITrainerIds
    {
        ushort TID16 { get; }
        ushort SID16 { get; }
    }
}
