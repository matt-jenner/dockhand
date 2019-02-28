using System;
using System.Threading.Tasks;
using Dockhand.Models;
using NUnit.Framework;

namespace Dockhand.Test
{
    public class Tests
    {
        //[SetUp]
        //public async Task Setup()
        //{
        //    var client = Client.DockerClient.ForDirectory(@"c:\Repos\C#\Just.Anarchy");
        //    //var image = await client.BuildImageAsync(@"Just.ContainedAnarchy\Dockerfile", "final", "justcontainedanarchy", "test");
        //    var image = await client.GetImageAsync("justcontainedanarchy", "test");
        //    var container = await image.StartContainerAsync(new[] { new DockerPortMapping(80, 5000) });
        //    var stats = await container.MonitorStatsFor(TimeSpan.FromSeconds(60));
        //    await container.KillAsync();
        //    await container.RemoveAsync();
        //    //await image.RemoveAsync();
        //}

        //[Test]
        //public async Task Test1()
        //{

        //}
    }
}