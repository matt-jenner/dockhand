using System;
using System.Collections.Generic;
using System.Linq;
using Dockhand.Utils.Extensions;

namespace Dockhand.Models
{
    public class ContainerStatsObservation
    {
        private readonly IList<DockerContainerStat> _stats;

        public ContainerStatsObservation(IEnumerable<DockerContainerStat> stats)
        {
            var containerStats = stats.ToList();
            if (containerStats.IsEmpty())
            {
                throw new ArgumentException("The collection of stats provided was empty.");
            }

            _stats = containerStats;
        }

        public decimal AverageCpu() => _stats.Average(x => x.Cpu);
        public decimal AverageMem() => _stats.Average(x => x.Memory);
        public decimal MaxCpu() => _stats.Max(x => x.Cpu);
        public decimal MaxMem() => _stats.Max(x => x.Memory);       
    }
}
