using System;
using System.Collections.Generic;

namespace Emulator
{
    public class CPU
    {
        public readonly Registers Registers = new Registers();
        public readonly Memory Memory = new Memory();

        public uint PC { get => Registers.PC; set => Registers.PC = value; }

        public CPU()
        {
        }

        private void ExecuteInstruction()
        {
            if (Registers.Thumb)
                ExecuteThumbInstruction();
            else
                ExecuteArmInstruction();
        }

        private void ExecuteThumbInstruction()
        {
            throw new NotImplementedException();
        }

        private void ExecuteArmInstruction()
        {
            var instruction = Memory.Get32(PC);
            if ((instruction & 0x0E000010) == 0)
            {
                // Data processing immediate shift
                PC += 4;
                ArmDataProcessing(instruction);
            }
            else if ((instruction & 0x0F900010) == 0x01000000)
            {
                // Misc, figure A3-4
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000090) == 0x00000010 && (instruction & 0x01900000) != 0x01000000)
            {
                // Data processing register shift
                PC += 4;
                ArmDataProcessing(instruction);
            }
            else if ((instruction & 0x0F900090) == 0x01000010)
            {
                // Misc, figure A3-4
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000090) == 0x00000090)
            {
                // Multiplies: A3-3, Extra load/stores A3-5
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000000) == 0x02000000 && (instruction & 0x01900000) != 0x01000000)
            {
                // Data processing immediate
                PC += 4;
                ArmDataProcessing(instruction);
            }
            else if ((instruction & 0x0FB00000) == 0x03000000)
            {
                // Undefined instruction
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0FB00000) == 0x03200000)
            {
                // Move immediate to status register
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000000) == 0x04000000)
            {
                // Load/store immediate offset
                PC += 4;
                ArmLoadStoreImmediateOffset(instruction);
            }
            else if ((instruction & 0x0E000010) == 0x06000000)
            {
                // Load/store register offset
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000010) == 0x06000010)
            {
                // Media instructions A3-2
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0FF000F0) == 0x07F000F0)
            {
                // Architecturally undefined
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000000) == 0x08000000)
            {
                // Load/store multiple
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0E000000) == 0x0A000000)
            {
                // Branch, Branch with link
                PC += 4;
                ArmBranch(instruction);
            }
            else if ((instruction & 0x0E000000) == 0x0C000000)
            {
                // Coprocessor load/store, double register transfers
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0F000010) == 0x0E000000)
            {
                // Coprocessor data processing
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0F000010) == 0x0E000010)
            {
                // Coprocessor register processing
                PC += 4;
                throw new NotImplementedException();
            }
            else if ((instruction & 0x0F000000) == 0x0F000000)
            {
                // Software interrupt
                PC += 4;
                throw new NotImplementedException();
            }
            else
            {
                throw new InvalidOperationException($"Got instruction {instruction:X8}, it didn't fit an ISA archetype.");
            }
        }

        private void ArmLoadStoreImmediateOffset(uint instruction)
        {

        }

        private void ArmDataProcessing(uint instruction)
        {
            if (!ConditionPassed(instruction))
                return;
            uint I = instruction & (1 << 25);
            uint opcode = (instruction & 0x01E00000) >> 21;
            uint S = instruction & (1 << 20);
            uint Rn = (instruction & 0x000F0000) >> 16;
            uint Rd = (instruction & 0x0000F000) >> 12;
            switch (opcode) {
                case 4:
                {
                    var (shifter_operand, c) = ShifterOperand(instruction);
                    Registers[Rd] = Registers[Rn] + shifter_operand;
                    if (S != 0 && Rd == 15)
                    {
                        if (Registers.CurrentModeHasSPSR)
                            Registers.CPSR = Registers.SPSR;
                        else
                            throw new InvalidOperationException("Unpredicatble.");
                    }
                    else if (S == 1)
                    {
                        Registers.N = (Registers[Rd] & (1 << 31)) != 0;
                        Registers.Z = Registers[Rd] == 0;
                        Registers.C = ((Registers[Rn] ^ shifter_operand) >= 0) & ((Registers[Rn] ^ Registers[Rd]) < 0);
                        Registers.V = (Registers[Rn] & (1 << 31)) == (shifter_operand & (1 << 31)) &&
                                      (Registers[Rn] & (1 << 31)) != (Registers[Rd] & (1 << 31));
                    }
                    break;
                }
                case 8:
                    if (S == 0)
                    {
                        ArmExtension(instruction);
                        return;
                    }
                    throw new NotImplementedException();
                case 9:
                    if (S == 0)
                    {
                        ArmExtension(instruction);
                        return;
                    }
                    throw new NotImplementedException();
                case 10:
                    if (S == 0)
                    {
                        ArmExtension(instruction);
                        return;
                    }
                    throw new NotImplementedException();
                case 11:
                    if (S == 0)
                    {
                        ArmExtension(instruction);
                        return;
                    }
                    throw new NotImplementedException();
                case 13:
                {
                    var (shifter_operand, c) = ShifterOperand(instruction);
                    Registers[Rd] = shifter_operand;
                    if (S != 0 && Rd == 15)
                    {
                        //TODO restore CPSR
                    }
                    else if (S != 0)
                    {
                        Registers.N = Registers[Rd] > 0x7FFFFFFF;
                        Registers.Z = Registers[Rd] == 0;
                        Registers.C = c;
                    }
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private void ArmExtension(uint instruction)
        {
            bool isControlDspExtension =
                (instruction & (0b11 << 26)) == 0 &&
                (instruction & (0b11 << 23)) == (0b10 << 23) &&
                (instruction & (1 << 20)) == 0 &&
                !(
                    (instruction & (1 << 25)) == 0 &&
                    (instruction & (1 << 7)) != 0 &&
                    (instruction & (1 << 4)) != 0
                );
            if (isControlDspExtension)
                ArmControlDspExtension(instruction);
        }

        private void ArmControlDspExtension(uint instruction)
        {
            if ((instruction & (1 << 25)) != 0)
            {
                // move immediate to status register
                throw new NotImplementedException();
                return;
            }
            var opcode = (instruction & (0b1111 << 4)) >> 4;
            switch (opcode) {
                case 0:
                {
                    var R = instruction & (1 << 22);
                    var field_mask = (instruction & (0b1111 << 16)) >> 16;
                    if ((instruction & (1 << 21)) == 0)
                    {
                        // move status register to register
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // move register to status register
                        var Rm = instruction & 0b1111;
                        const uint UnallocMask = 0x0FFFFF00;
                        const uint UserMask = 0xF0000000;
                        const uint PrivMask = 0x0000000F;
                        const uint StateMask = 0x00000020;
                        var operand = Registers[Rm];
                        if ((operand & UnallocMask) != 0)
                            throw new InvalidOperationException("Unpredictable. (attempt to set reserved bits)");
                        var byte_mask =
                            ((field_mask & 0b0001) != 0 ? 0x000000FFu : 0) |
                            ((field_mask & 0b0010) != 0 ? 0x0000FF00u : 0) |
                            ((field_mask & 0b0100) != 0 ? 0x00FF0000u : 0) |
                            ((field_mask & 0b1000) != 0 ? 0xFF000000u : 0);
                        uint mask;
                        if (R == 0) {
                            if (Registers.InAPrivilegedMode)
                            {
                                if ((operand & StateMask) != 0)
                                    throw new InvalidOperationException("Unpredicatble. (attempt to set non-ARM execution state)");
                                else
                                    mask = byte_mask & (UserMask | PrivMask);     
                            }
                            else
                                mask = byte_mask & UserMask;
                            Registers.CPSR = (Registers.CPSR & ~mask) | (operand & mask);
                        } else {
                            if (Registers.CurrentModeHasSPSR)
                            {
                                mask = byte_mask & (UserMask | PrivMask | StateMask);
                                Registers.SPSR = (Registers.SPSR & ~mask) | (operand & mask);
                            }
                            else
                                throw new InvalidOperationException("Unpredicatble. (attempt to set SPSR where the current mode does not have one)");
                        }
                        break;
                    }
                }
                case 1:
                    if ((instruction & (1 << 22)) == 0)
                    {
                        // branch/exchange instruction set Thumb
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // count leading zeroes
                        throw new NotImplementedException();
                    }
                case 2:
                    // branch/exchange instruction set Java
                    throw new NotImplementedException();
                case 3:
                    // branch and link/exchangee instruction set Thumb
                    throw new NotImplementedException();
                case 5:
                    // saturating add/subtract
                    throw new NotImplementedException();
                case 7:
                    // software breakpoint
                    throw new NotImplementedException();
                default:
                    if (opcode >= 8) {
                        // signed multiplies (type 2)
                        throw new NotImplementedException();
                    }
                    throw new InvalidOperationException("Invalid control or DSP extension instruction");
            }
        }

        private void ArmBranch(uint instruction)
        {
            if (!ConditionPassed(instruction))
                return;
            uint L = instruction & (1 << 24);
            if (L != 0)
                Registers.LR = PC + 4;
            var offset = (((int)(instruction & 0x00FFFFFF)) << 2);
            if ((instruction & 0x00800000) != 0)
                offset |= 0xFF << 24; // sign extend
            PC = (uint)(PC + offset + 4);
        }

        private bool ConditionPassed(uint instruction)
        {
            return true;
        }

        private (uint, bool) ShifterOperand(uint instruction)
        {
            // bit 25 indicates immediate
            if ((instruction & (1 << 25)) == 0)
            {
                var Rm = instruction & 0xF;
                if ((instruction & (0xFF << 4)) == 0)
                {
                    // shortcut: Just use the register
                    return (Registers[Rm], Registers.C);
                }
                if ((instruction & (0b111 << 4)) == 0)
                {
                    // logical shift left, immediate
                    int shift_imm = (int)((instruction & (0xF << 7)) >> 7);
                    // no need to worry about shifter_imm == 0; the above If takes care of it.
                    var shifter_operand = Registers[Rm] << shift_imm;
                    var shifter_carry_out = (Registers[Rm] & (1 << (32 - shift_imm))) != 0;
                    return (shifter_operand, shifter_carry_out);
                }
                else if ((instruction & (0b111 << 4)) == 0b0100000)
                {
                    // logical shift right, immediate
                    int shift_imm = (int)((instruction & (0xF << 7)) >> 7);
                    if (shift_imm == 0)
                    {
                        return (0, (Registers[Rm] & (1 << 31)) != 0);
                    }
                    else
                    {
                        var shifter_operand = Registers[Rm] >> shift_imm;
                        var shifter_carry_out = (Registers[Rm] & (1 << (shift_imm - 1))) != 0;
                        return (shifter_operand, shifter_carry_out);
                    }
                }
                else if ((instruction & 0b11110000) == 0b00010000)
                {
                    // logical shift left, register
                    var Rs = (instruction & (0xF << 8)) >> 8;
                    var RsValue = (int)(Registers[Rs] & 0xFF);
                    uint shifter_operand;
                    bool shifter_carry_out;
                    if (RsValue == 0)
                    {
                        shifter_operand = Registers[Rm];
                        shifter_carry_out = Registers.C;
                    }
                    else if (RsValue < 32)
                    {
                        shifter_operand = Registers[Rm] << RsValue;
                        shifter_carry_out = (Registers[Rm] & (1 << (32 - RsValue))) != 0;
                    }
                    else if (RsValue == 32)
                    {
                        shifter_operand = 0;
                        shifter_carry_out = (Registers[Rm] & 1) != 0;
                    }
                    else
                    {
                        shifter_operand = 0;
                        shifter_carry_out = false;
                    }
                    return (shifter_operand, shifter_carry_out);
                }
                else if ((instruction & 0b11110000) == 0b00110000)
                {
                    // logical shift right, register
                    var Rs = (instruction & (0xF << 8)) >> 8;
                    var RsValue = (int)(Registers[Rs] & 0xFF);
                    uint shifter_operand;
                    bool shifter_carry_out;
                    if (RsValue == 0)
                    {
                        shifter_operand = Registers[Rm];
                        shifter_carry_out = Registers.C;
                    }
                    else if (RsValue < 32)
                    {
                        shifter_operand = Registers[Rm] >> RsValue;
                        shifter_carry_out = (Registers[Rm] & (1 << (RsValue - 1))) != 0;
                    }
                    else if (RsValue == 32)
                    {
                        shifter_operand = 0;
                        shifter_carry_out = (Registers[Rm] & (1 << 31)) != 0;
                    }
                    else
                    {
                        if ((Registers[Rm] & (1 << 31)) == 0)
                        {
                            shifter_operand = 0;
                            shifter_carry_out = false;
                        }
                        else
                        {
                            shifter_operand = 0xFFFFFFFF;
                            shifter_carry_out = true;
                        }
                    }
                    return (shifter_operand, shifter_carry_out);
                }
                else if ((instruction & (0b111 << 4)) == 0b1100000)
                {
                    // rotate right, immediate
                    int shift_imm = (int)((instruction & (0xF << 7)) >> 7);
                    if (shift_imm == 0)
                    {
                        var shifter_operand = ((Registers.C ? 0u : 1) << 31) | (Registers[Rm] >> 1);
                        var shifter_carry_out = (Registers[Rm] & 1) != 0;
                        return (shifter_operand, shifter_carry_out);
                    }
                    else
                    {
                        var shifter_operand = Registers[Rm].RotateRight(shift_imm);
                        var shifter_carry_out = (Registers[Rm] & (1 << (shift_imm - 1))) != 0;
                        return (shifter_operand, shifter_carry_out);
                    }
                }
                else if ((instruction & 0b11110000) == 0b01110000)
                {
                    // rotate right, register
                    var Rs = (instruction & (0xF << 8)) >> 8;
                    var RsValue = (int)(Registers[Rs] & 0xFF);
                    var RsWidth5 = RsValue & 0b11111;
                    uint shifter_operand;
                    bool shifter_carry_out;
                    if (RsValue == 0)
                    {
                        shifter_operand = Registers[Rm];
                        shifter_carry_out = Registers.C;
                    }
                    else if (RsWidth5 == 0)
                    {
                        shifter_operand = Registers[Rm];
                        shifter_carry_out = (Registers[Rm] & (1 << 31)) != 0;
                    }
                    else
                    {
                        shifter_operand = Registers[Rm].RotateRight(RsWidth5);
                        shifter_carry_out = (Registers[Rm] & (1 << (RsWidth5 - 1))) != 0;
                    }
                    return (shifter_operand, shifter_carry_out);
                }
                else if ((instruction & 0b111111110000) == 0b00000110000)
                {
                    var shifter_operand = ((Registers.C ? 0u : 1) << 31) | (Registers[Rm] >> 1);
                    var shifter_carry_out = (Registers[Rm] & 1) != 0;
                    return (shifter_operand, shifter_carry_out);
                }
                throw new InvalidOperationException("Unrecognized shifter operand");
            }
            else
            {
                uint rotate_imm = (instruction & 0x0000F00) >> 8;
                uint immed_8 = instruction & 0x000000FF;
                uint shifter_operand = immed_8.RotateRight((int)rotate_imm * 2);
                return (shifter_operand, rotate_imm == 0 ? Registers.C : ((shifter_operand & 0x80000000) != 0));
            }
        }

        public void Run()
        {
            while (true)
            {
                ExecuteInstruction();
            }
        }
    }
}
