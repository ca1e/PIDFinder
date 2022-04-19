namespace PIDFinder
{
    public interface IRNG
    {
        uint Next(uint seed);

         PKM GenPkm(uint seed, ITrainerID trainer);
    }

    enum MethodType
    {
        Method1,
        Roaming8b,
    }
}