using System;
using System.Collections.Generic;
using System.Text;

namespace Dockhand.Test.Builders
{
    internal static class MotherFor
    {
        internal static MockCommandFactoryBuilder CommandFactory => new MockCommandFactoryBuilder();

        internal static MockCommandWrapperBuilder CommandWrapper => new MockCommandWrapperBuilder();

        internal static ContainerStatsJsonBuilder ContainerStatsJson(decimal cpu, decimal mem) => new ContainerStatsJsonBuilder(cpu, mem);

        internal static ObservationsJsonBuilder ObservationsJsonBuilder => new ObservationsJsonBuilder();
    }
}
