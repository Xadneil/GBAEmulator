using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Emulator
{
    public class Memory
    {
        private readonly byte[] bios = new byte[16 * 1024];
        private readonly byte[] wramBoard = new byte[256 * 1024];
        private readonly byte[] wramChip = new byte[32 * 1024];

        private readonly byte[] palette = new byte[1 * 1024];
        private readonly byte[] vram = new byte[96 * 1024];
        private readonly byte[] attributes = new byte[1 * 1024];
        private byte[] rom;

        private byte this[uint address]
        {
            get
            {
                if (address <= 0x3FFF)
                    return bios[address];
                if (address >= 0x02000000 && address <= 0x0203FFFF)
                    return wramBoard[address - 0x02000000];
                if (address >= 0x03000000 && address <= 0x03007FFF)
                    return wramChip[address - 0x03000000];
                // I/O
                if (address >= 0x05000000 && address <= 0x050003FF)
                    return palette[address - 0x05000000];
                if (address >= 0x06000000 && address <= 0x06017FFF)
                    return vram[address - 0x06000000];
                if (address >= 0x07000000 && address <= 0x070003FF)
                    return attributes[address - 0x07000000];
                // external memory
                if (address >= 0x08000000 && rom != null && address <= 0x08000000 + rom.Length)
                    return rom[address - 0x08000000];
                if (address >= 0x0A000000 && rom != null && address <= 0x0A000000 + rom.Length)
                    return rom[address - 0x0A000000];
                if (address >= 0x0C000000 && rom != null && address <= 0x0C000000 + rom.Length)
                    return rom[address - 0x0C000000];

                throw new NotImplementedException();
            }
            set
            {
                if (address <= 0x3FFF)
                    bios[address] = value;
                else if (address >= 0x02000000 && address <= 0x0203FFFF)
                    wramBoard[address - 0x02000000] = value;
                else if (address >= 0x03000000 && address <= 0x03007FFF)
                    wramChip[address - 0x03000000] = value;
                // I/O
                else if (address >= 0x05000000 && address <= 0x050003FF)
                    palette[address - 0x05000000] = value;
                else if (address >= 0x06000000 && address <= 0x06017FFF)
                    vram[address - 0x06000000] = value;
                else if (address >= 0x76000000 && address <= 0x070003FF)
                    attributes[address - 0x07000000] = value;
                // external memory
                if (address >= 0x08000000 && rom != null && address <= 0x08000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");
                if (address >= 0x0A000000 && rom != null && address <= 0x0A000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");
                if (address >= 0x0C000000 && rom != null && address <= 0x0C000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");

                throw new NotImplementedException();
            }
        }

        public uint Get32(uint address)
        {
            var byte1 = this[address];
            var byte2 = (uint)this[address + 1] << 8;
            var byte3 = (uint)this[address + 2] << 16;
            var byte4 = (uint)this[address + 3] << 24;
            return byte1 + byte2 + byte3 + byte4;
        }

        public void Set32(uint address, uint value)
        {
            byte byte1 = (byte)value; // truncates
            byte byte2 = (byte)(value >> 8);
            byte byte3 = (byte)(value >> 16);
            byte byte4 = (byte)(value >> 24);
            this[address] = byte1;
            this[address + 1] = byte2;
            this[address + 2] = byte3;
            this[address + 3] = byte4;
        }

        public void LoadRom(byte[] rom)
        {
            this.rom = rom;
        }
    }
}
