using System;

namespace SystemInfo.Core.Abstractions
{
    public interface IMemoryOptimizer
    {
        string Name { get; }
        void Optimize();
    }
}
