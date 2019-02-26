using System.Diagnostics.CodeAnalysis;
using Dockhand.Utils;
using Newtonsoft.Json;

namespace Dockhand.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ContainerStat
    {
        [JsonConverter(typeof(DockerPercentStringConverter))]
        public decimal cpu { get; set; }
        [JsonConverter(typeof(DockerPercentStringConverter))]
        public decimal mem { get; set; }
    }
}
