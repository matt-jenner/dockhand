using System;
using Newtonsoft.Json;

namespace Dockhand.Models
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
