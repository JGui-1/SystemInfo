namespace SystemInfo.Core.Abstractions
{
    public interface IDiskOptimizer
    {
        void Optimize(string driveLetter, int powerState);
    }
}
