using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Memory
{
    // Reserva e libera blocos para incentivar o sistema a liberar listas de standby.
    public class TlbFlusher : IMemoryOptimizer
    {
        public string Name => "TLB/Standby Flusher";

        [Flags]
        private enum AllocationType : uint { Commit = 0x1000, Reserve = 0x2000 }
        [Flags]
        private enum MemoryProtection : uint { ReadWrite = 0x04 }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        public void Optimize()
        {
            const int totalMB = 512;      // não exagerar
            const int chunkMB = 16;
            var allocated = new List<IntPtr>();

            try
            {
                for (int done = 0; done < totalMB; done += chunkMB)
                {
                    var size = new UIntPtr((ulong)chunkMB * 1024 * 1024);
                    IntPtr ptr = VirtualAlloc(IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);
                    if (ptr == IntPtr.Zero) break;
                    allocated.Add(ptr);
                    Span<byte> s = new byte[1];
                    Marshal.Copy(new byte[] { 0 }, 0, ptr, 1); // tocar na página
                }
                Console.WriteLine($"[TlbFlusher] Reservado ~{allocated.Count * chunkMB} MB para forçar paginação; liberando...");
            }
            finally
            {
                foreach (var ptr in allocated)
                    VirtualFree(ptr, UIntPtr.Zero, 0x8000); // MEM_RELEASE
            }
        }
    }
}
