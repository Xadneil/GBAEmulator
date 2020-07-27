using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public class Emulator
    {
        private readonly CPU CPU = new CPU();

        public void LoadROM(byte[] rom)
        {
            CPU.Memory.LoadRom(rom);
        }

        public void Run()
        {
            CPU.Run();
        }
    }
}
