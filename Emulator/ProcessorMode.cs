using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public enum ProcessorMode : uint
    {
        User = 0b10000,
        FIQ = 0b10001,
        IRQ = 0b10010,
        Supervisor = 0b10011,
        Abort = 0b10111,
        Undefined = 0b11011,
        System = 0b11111
    }
}
