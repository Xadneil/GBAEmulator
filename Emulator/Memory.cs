using System;

namespace Emulator
{
    public class Memory
    {
        private readonly byte[] bios = new byte[16 * 1024];
        private readonly byte[] wramBoard = new byte[256 * 1024];
        private readonly byte[] wramChip = new byte[32 * 1024];
        private readonly byte[] ioRegisters = new byte[0x400];

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
                if (address >= 0x04000000 && address <= 0x040003FE)
                    return ioRegisters[address - 0x04000000];
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
                else if (address >= 0x04000000 && address <= 0x040003FE)
                    ioRegisters[address - 0x04000000] = value;
                else if (address >= 0x05000000 && address <= 0x050003FF)
                    palette[address - 0x05000000] = value;
                else if (address >= 0x06000000 && address <= 0x06017FFF)
                    vram[address - 0x06000000] = value;
                else if (address >= 0x70000000 && address <= 0x070003FF)
                    attributes[address - 0x07000000] = value;
                // external memory
                else if (address >= 0x08000000 && rom != null && address <= 0x08000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");
                else if (address >= 0x0A000000 && rom != null && address <= 0x0A000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");
                else if (address >= 0x0C000000 && rom != null && address <= 0x0C000000 + rom.Length)
                    throw new InvalidOperationException("Cannot write to ROM");
                else
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

        public ushort Get16(uint address)
        {
            var byte1 = this[address];
            var byte2 = this[address + 1] << 8;
            return (ushort)(byte1 + byte2);
        }

        public void Set32(uint address, uint value)
        {
            address &= ~(uint)0b11; // ignore bottom 2 bits of address for alignment

            byte byte1 = (byte)value; // truncates
            byte byte2 = (byte)(value >> 8);
            byte byte3 = (byte)(value >> 16);
            byte byte4 = (byte)(value >> 24);
            this[address] = byte1;
            this[address + 1] = byte2;
            this[address + 2] = byte3;
            this[address + 3] = byte4;
        }

        public void Set16(uint address, uint value)
        {
            address &= ~(uint)1; // ignore bottom bit of address for alignment

            byte byte1 = (byte)value; // truncates
            byte byte2 = (byte)(value >> 8);
            this[address] = byte1;
            this[address + 1] = byte2;
        }

        public void LoadRom(byte[] rom)
        {
            this.rom = rom;
        }
    }
}
