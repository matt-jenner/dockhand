namespace Dockhand.Test.Builders
{
    internal class ContainerStatsJsonBuilder
    {
        private decimal _cpu;
        private decimal _mem;

        internal ContainerStatsJsonBuilder(decimal cpu, decimal mem)
        {
            _cpu = cpu;
            _mem = mem;
        }

        internal string Build() => $"{{\"cpu\":\"{_cpu:0.##}%\",\"mem\":\"{_mem:0.##}%\"}}";
    }
}
