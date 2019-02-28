using System;
using System.Collections.Generic;
using System.Linq;
using Dockhand.Dtos;
using Dockhand.Utils.Extensions;
using Newtonsoft.Json;

namespace Dockhand.Models
{
    public class ContainerStatsObservation
    {
        private IEnumerable<ContainerStat> _stats;

        public ContainerStatsObservation(IEnumerable<string> responseLines)
        {
            _stats = responseLines.Select(line =>
            {
                var containerStat = JsonConvert.DeserializeObject<ContainerStat>(line);

                ValidateStat(containerStat);

                return containerStat;
            }).ToList();

            if (_stats.IsEmpty())
            {
                throw new ArgumentException("The collection of response lines provided were empty.");
            }
        }

        public decimal AverageCpu() => _stats.Average(x => x.cpu);
        public decimal AverageMem() => _stats.Average(x => x.mem);
        public decimal MaxCpu() => _stats.Max(x => x.cpu);
        public decimal MaxMem() => _stats.Max(x => x.mem);

        private void ValidateStat(ContainerStat stat)
        {
            if (stat.cpu < 0)
                ThrowNegativeStatException("cpu", stat.cpu);
            
            if (stat.mem < 0)
                ThrowNegativeStatException("mem", stat.mem);
        }

        private void ThrowNegativeStatException(string statName, decimal value) => throw new ArgumentException($"The {statName} stat should not ever be negative, but encountered negative value: {value}");
    }
}
