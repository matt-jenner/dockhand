using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dockhand.Test.Builders
{
    internal class ObservationsJsonBuilder
    {
        private IList<decimal> _cpus;
        private IList<decimal> _mems;

        internal ObservationsJsonBuilder()
        {
            _cpus = new decimal[0];
            _mems = new decimal[0];
        }

        internal ObservationsJsonBuilder WithCpuReadings(params decimal[] readings)
        {
            _cpus = readings;
            return this;
        }

        internal ObservationsJsonBuilder WithMemReadings(params decimal[] readings)
        {
            _mems = readings;
            return this;
        }


        internal IEnumerable<string> Build()
        {
            if (_mems.Count == 0)
            {
                _mems = Enumerable.Repeat(0m, _cpus.Count).ToArray();
            }

            if (_cpus.Count == 0)
            {
                _cpus = Enumerable.Repeat(0m, _mems.Count).ToArray();
            }

            if (_cpus.Count != _mems.Count)
            {
                throw new ArgumentException("The number of CPU and Memory readings do not match up!");
            }

            return _cpus.Select((c, i) => new ContainerStatsJsonBuilder(c, _mems[i]).Build());
        } 
    }
}
