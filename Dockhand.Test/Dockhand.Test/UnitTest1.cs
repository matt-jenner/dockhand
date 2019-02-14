using System;
using System.Linq;
using System.Threading.Tasks;
using Dockhand;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public async Task Setup()
        {
            var dcm = new DockerContainerManager(@"c:\Repos\C#\Just.Anarchy");
            await dcm.BuildImageAsync(@"Just.ContainedAnarchy\Dockerfile", "final","justcontainedanarchy", "test");
            var imageResult = (await dcm.GetImagesAsync()).First(x => x.Repository == "justcontainedanarchy" && x.Tag == "test");
            var containerId = await dcm.StartContainerAsync(imageResult.Id);
            var stats = dcm.GetContainerStats(containerId, TimeSpan.FromSeconds(60));
            await dcm.KillContainerAsync(containerId);
            await dcm.RemoveContainer(containerId);
            await dcm.RemoveImage(imageResult.Id);
        }

        [Test]
        public async Task Test1()
        {

            
        }
    }
}