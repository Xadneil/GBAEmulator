using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public class Registers
    {
        /// <summary>
        /// R0-R15 (16 registers), R8_fiq#16-R14_fiq#22 (7 registers),
        /// R13_svc#23, R14_svc#24, R13_abt#25, R14_abt#26, R13_irq#27,
        /// R14_irq#28, R13_und#29, R14_und#30,
        /// CPSR#31, SPSR_fiq#32, SPSR_svc#33, SPSR_abt#34, SPSR_irq#35, SPSR_und#36
        /// </summary>
        private uint[] registers;

        private readonly int[,] registerBankMap = new int[16, 7]
        {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 2, 2, 2, 2, 2, 2, 2 },
            { 3, 3, 3, 3, 3, 3, 3 },
            { 4, 4, 4, 4, 4, 4, 4 },
            { 5, 5, 5, 5, 5, 5, 5 },
            { 6, 6, 6, 6, 6, 6, 6 },
            { 7, 7, 7, 7, 7, 7, 7 },
            { 8, 16, 8, 8, 8, 8, 8 },
            { 9, 17, 9, 9, 9, 9, 9 },
            { 10, 18, 10, 10, 10, 10, 10 },
            { 11, 19, 11, 11, 11, 11, 11 },
            { 12, 20, 12, 12, 12, 12, 12 },
            { 13, 21, 27, 23, 25, 29, 13 },
            { 14, 22, 28, 24, 26, 30, 14 },
            { 15, 15, 15, 15, 15, 15, 15 },
        };

        private readonly int[] spsrBank =
            new[] { -1, 32, 33, 34, 35, 36, -1 };

        public Registers()
        {
            Reset();
        }

        public void Reset()
        {
            registers = new uint[37];
            PC = 0x08000000;
            CPSR = 0b111010011;
        }

        private int ModeIndex => Mode switch
        {
            ProcessorMode.User => 0,
            ProcessorMode.FIQ => 1,
            ProcessorMode.IRQ => 2,
            ProcessorMode.Supervisor => 3,
            ProcessorMode.Abort => 4,
            ProcessorMode.Undefined => 5,
            ProcessorMode.System => 6,
            _ => throw new InvalidOperationException()
        };

        public uint this[uint index]
        {
            get
            {
                if (index == 15)
                    return registers[15] + (Thumb ? 4u : 8);
                return registers[registerBankMap[index, ModeIndex]];
            }
            set => registers[registerBankMap[index, ModeIndex]] = value;
        }

        public uint LR
        {
            get => registers[14];
            set => registers[14] = value;
        }

        public uint PC { get => registers[15]; set => registers[15] = value; }
        public uint CPSR { get => registers[31]; set => registers[31] = value; }
        public uint SPSR { get => registers[spsrBank[ModeIndex]]; set => registers[spsrBank[ModeIndex]] = value; }
        public bool N
        {
            get => (registers[31] & (1 << 31)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 31;
                else
                    registers[31] &= ~(1u << 31);
            }
        }
        public bool Z
        {
            get => (registers[31] & (1 << 30)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 30;
                else
                    registers[31] &= ~(1u << 30);
            }
        }
        public bool C
        {
            get => (registers[31] & (1 << 29)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 29;
                else
                    registers[31] &= ~(1u << 29);
            }
        }
        public bool V
        {
            get => (registers[31] & (1 << 28)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 28;
                else
                    registers[31] &= ~(1u << 28);
            }
        }
        public bool FIQDisable
        {
            get => (registers[31] & (1 << 7)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 7;
                else
                    registers[31] &= ~(1u << 7);
            }
        }
        public bool IRQDisable
        {
            get => (registers[31] & (1 << 6)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 6;
                else
                    registers[31] &= ~(1u << 6);
            }
        }
        public bool Thumb
        {
            get => (registers[31] & (1 << 5)) != 0;
            set
            {
                if (value)
                    registers[31] |= 1u << 5;
                else
                    registers[31] &= ~(1u << 5);
            }
        }
        public ProcessorMode Mode { get => (ProcessorMode)((registers[31]) & 0x1F); }
        public bool InAPrivilegedMode { get => Mode != ProcessorMode.User; }
        public bool CurrentModeHasSPSR { get => Mode != ProcessorMode.User && Mode != ProcessorMode.System; }
    }
}
