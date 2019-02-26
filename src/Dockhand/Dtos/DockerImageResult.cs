using System.Diagnostics.CodeAnalysis;

namespace Dockhand.Dtos
{
    [ExcludeFromCodeCoverage]
    internal class DockerImageResult
    {
        public string Repository { get; set; }
        public string Tag { get; set; }
        public string Id { get; set; }
    }
}
