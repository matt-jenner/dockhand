using System;
using System.Collections.Generic;
using System.Text;

namespace Dockhand.Models
{
    public class DockerImageResult
    {
        public string Repository { get; set; }
        public string Tag { get; set; }
        public string Id { get; set; }
    }
}
