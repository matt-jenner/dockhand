using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                return containerStat;
            }).ToList();
        }

        public ContainerStatsObservation(IEnumerable<ContainerStat> stats)
        {
            _stats = stats;
        }

        public decimal AverageCpu() => _stats.Average(x => x.cpu);
        public decimal AverageMem() => _stats.Average(x => x.mem);
        public decimal MaxCpu() => _stats.Max(x => x.cpu);
        public decimal MaxMem() => _stats.Max(x => x.mem);
    }
}
