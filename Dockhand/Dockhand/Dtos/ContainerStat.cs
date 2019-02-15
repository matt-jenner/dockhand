using System;
using Dockhand.Utils;
using Newtonsoft.Json;

namespace Dockhand.Dtos
{
    public class ContainerStat
    {
        public DateTime ObservationDateTime { get; set; }
        [JsonConverter(typeof(DockerPercentStringConverter))]
        public decimal cpu { get; set; }
        [JsonConverter(typeof(DockerPercentStringConverter))]
        public decimal mem { get; set; }
    }
}
