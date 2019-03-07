using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dockhand.Interfaces;
using Dockhand.Utils;
using Medallion.Shell;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Dockhand.Test.Utils
{
    [TestFixture]
    public class CommandFactoryTests
    {
        [Test]
        [TestCase("somethingwith/inIt", "somethingwith\\inIt")]
        [TestCase("endswith/", "endswith\\")]
        [TestCase("/startswith", "\\startswith")]
        [TestCase("/", "\\")]
        public void RunCommandSwitchesBackslashesForWindows(string command, string formattedCommand)
        {
            // Arrange
            var mockShell = GetCancellableShell();
            var mockShellFactory = GetShellFactory(mockShell);
            var mockDockhandEnvironment = GetDockhandEnvironment(true);
            var sut = new CommandFactory(mockShellFactory, mockDockhandEnvironment);

            // Act
            sut.RunCommand(command, "anyoldworkingdirectory");

            // Assert
            mockShell.Received(1).Run("cmd.exe", "/C", formattedCommand);
        }

        [Test]
        [TestCase("somethingwith\\inIt", "somethingwith/inIt")]
        [TestCase("endswith\\", "endswith/")]
        [TestCase("\\startswith", "/startswith")]
        [TestCase("\\","/")]
        public void RunCommandSwitchesSlashesForLinux(string command, string formattedCommand)
        {
            // Arrange
            var mockShell = GetCancellableShell();
            var mockShellFactory = GetShellFactory(mockShell);
            var mockDockhandEnvironment = GetDockhandEnvironment(false);
            var sut = new CommandFactory(mockShellFactory, mockDockhandEnvironment);

            // Act
            sut.RunCommand(command, "anyoldworkingdirectory");

            // Assert
            mockShell.Received(1).Run("/bin/bash", "-c", $"\"{formattedCommand}\"");
        }

        private IShellFactory GetShellFactory(ICancellableShell shell)
        {
            var mockShellFactory = Substitute.For<IShellFactory>();
            
            mockShellFactory.Create(Arg.Any<CancellationToken?>(), Arg.Any<string>()).Returns(shell);
            return mockShellFactory;
        }

        private ICancellableShell GetCancellableShell() => Substitute.For<ICancellableShell>();

        private IDockhandEnvironment GetDockhandEnvironment(bool isWindowsResult)
        {
            var mockDockhandEnvironment = Substitute.For<IDockhandEnvironment>();
            mockDockhandEnvironment.IsWindows.Returns(isWindowsResult);
            return mockDockhandEnvironment;
        }
    }
}
