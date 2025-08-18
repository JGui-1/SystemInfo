using System;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Output
{
    public class ConsoleOutput : IOutput
    {
        public void Write(string text) => Console.Write(text);
        public void WriteLine(string text = "") => Console.WriteLine(text);
        public string ReadLine() => Console.ReadLine();
        public ConsoleKeyInfo ReadKey() => Console.ReadKey();
    }
}
