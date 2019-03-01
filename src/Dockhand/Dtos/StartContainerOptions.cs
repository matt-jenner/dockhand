using System;
using System.Collections.Generic;
using Dockhand.Models;

namespace Dockhand.Dtos
{
    public class StartContainerOptions
    {
        internal int? MemoryLimitMb { get; private set; }
        internal decimal? CpuLimit { get; private set; }
        internal IList<DockerPortMapping> DockerPortMappings { get; private set; }

        internal StartContainerOptions()
        {
            DockerPortMappings = new List<DockerPortMapping>();
        }

        public StartContainerOptions WithMemoryLimit(int memoryLimitMb)
        {
            MemoryLimitMb = memoryLimitMb;
            return this;
        }

        public StartContainerOptions WithCpuLimit(decimal cpuCount)
        {
            CpuLimit = cpuCount;
            return this;
        }

        public StartContainerOptions ExposePort(int internalPort, int externalPort)
        {
            DockerPortMappings.Add(new DockerPortMapping(internalPort, externalPort));
            return this;
        }

        public StartContainerOptions ExposePorts(IEnumerable<DockerPortMapping> portMappings)
        {
            foreach (var dockerPortMapping in portMappings)
            {
                DockerPortMappings.Add(dockerPortMapping);
            }

            return this;
        }

        internal void Validate()
        {
            if (MemoryLimitMb.HasValue)
                MemoryLimitIsValidForContainer();

            if (CpuLimit.HasValue)
                CpuLimitIsValidForContainer();
        }

        private void CpuLimitIsValidForContainer()
        {
            var totalCoreCount = Environment.ProcessorCount;
            if (CpuLimit > totalCoreCount)
                throw new ArgumentException($"If a cpu limit is specified, it must be a positive decimal and be less than the total number of cpu cores available ({totalCoreCount}) .");
        }

        private void MemoryLimitIsValidForContainer()
        {
            if (MemoryLimitMb < 4)
                throw new ArgumentException("If a memory limit is specified, the minimum limit (specified in mb) that can be set is 4 mb");
        }
    }
}
