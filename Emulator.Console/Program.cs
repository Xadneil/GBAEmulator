using System;
using System.IO;

namespace Emulator.ConsoleApp
{
    class Program
    {
        static void Main()
        {
            var image = File.ReadAllBytes(@"C:\Users\Daniel\Downloads\tonc-bin\bin\first.gba");
            var emu = new Emulator();
            emu.LoadROM(image);
            emu.Run();
        }
    }
}
