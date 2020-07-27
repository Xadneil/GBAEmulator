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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                ArmDataProcessingImmediate(instruction);
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

        private void ArmDataProcessingImmediate(uint instruction)
        {
            if (!ConditionPassed(instruction))
                return;
            uint I = instruction & (1 << 25);
            uint opcode = (instruction & 0x01E00000) >> 21;
            uint S = instruction & (1 << 20);
            int Rn = (int)((instruction & 0x000F0000) >> 16);
            int Rd = (int)((instruction & 0x0000F000) >> 12);
            //TODO put this in a switch on opcode
            var (shifter_operand, c) = ShifterOperandImmediate(instruction);
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

        private (uint, bool) ShifterOperandImmediate(uint instruction)
        {
            uint rotate_imm = (instruction & 0x0000F00) >> 8;
            uint immed_8 = instruction & 0x000000FF;
            uint shifter_operand = immed_8.RotateRight((int)rotate_imm * 2);
            return (shifter_operand, rotate_imm == 0 ? Registers.C : ((shifter_operand & 0x80000000) != 0));
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
