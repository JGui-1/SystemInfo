using System;

namespace SystemInfo.Core.Abstractions
{
    public interface IOutput
    {
        void Write(string text);
        void WriteLine(string text = "");
        string ReadLine();
        ConsoleKeyInfo ReadKey();
    }
}
