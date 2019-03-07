using System;
using Dockhand.Dtos;

namespace Dockhand.Models
{
    public class DockerContainerStat
    {
        public decimal Cpu { get; private set; }
        public decimal Memory { get; private set; }


        internal DockerContainerStat(decimal cpu, decimal memory)
        {
            if (cpu < 0)
                ThrowNegativeStatException(nameof(cpu), cpu);

            if (memory < 0)
                ThrowNegativeStatException(nameof(memory), memory);

            Cpu = cpu;
            Memory = memory;
        }

        private void ThrowNegativeStatException(string statName, decimal value) => throw new ArgumentException($"The {statName} stat never be negative, but encountered negative value: {value}");
    }
}
