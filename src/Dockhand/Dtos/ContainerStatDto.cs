using System;
using System.Diagnostics.CodeAnalysis;
using Dockhand.Utils;
using Newtonsoft.Json;

namespace Dockhand.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ContainerStatDto
    {
        [JsonConverter(typeof(DockerPercentStringConverter))]
        [JsonProperty("cpu")]
        public decimal Cpu { get; set; }

        [JsonConverter(typeof(DockerPercentStringConverter))]
        [JsonProperty("mem")]
        public decimal Mem { get; set; }

        
    }
}
 