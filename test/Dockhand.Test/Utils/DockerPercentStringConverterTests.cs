using System;
using System.IO;
using Dockhand.Utils;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dockhand.Test.Utils
{
    [TestFixture]
    public class DockerPercentStringConverterTests
    {
        [Test]
        public void ItShouldNotReportItCanSerialize()
        {
            // Arrange
            var sut = new DockerPercentStringConverter();

            // Act/Assert
            sut.CanWrite.Should().BeFalse();
        }

        [Test]
        public void ItShouldReportItCanDeserialize()
        {
            // Arrange
            var sut = new DockerPercentStringConverter();

            // Act/Assert
            sut.CanRead.Should().BeTrue();
        }

        [Test]
        [TestCase(typeof(Decimal), true)]
        [TestCase(typeof(Decimal?), true)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        public void ItShouldOnlyBeAbleToConvertToDecimals(Type type, bool expectedResult)
        {
            // Arrange
            var sut = new DockerPercentStringConverter();

            // Act
            var result = sut.CanConvert(type);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("\"32.325%\"", 32.325)]
        [TestCase("\"0%\"", 0)]
        [TestCase("\"  5%\"", 5)]
        [TestCase("\"5\"", 5)]
        [TestCase("\"1\"", 1)]
        public void ItHandlesReadingValidDecimalStringValuesCorrectly(string json, decimal expectedResult)
        {
            // Arrange/Act
            var result = JsonConvert.DeserializeObject<Decimal>(json, new DockerPercentStringConverter());

            // Assert
            result.Should().Be(expectedResult);
        }

        [Test]
        public void ItHandlesNullableDecimalStringValuesCorrectly()
        {
            // Arrange/Act
            var result = JsonConvert.DeserializeObject<Decimal?>("null", new DockerPercentStringConverter());

            // Assert
            result.Should().BeNull();
        }


        [Test]
        [TestCase("\"I am not a number!\"")]
        [TestCase("\"%\"")]
        [TestCase("\"  %\"")]
        [TestCase("\"  %  %\"")]
        [TestCase("\"\t\"")]
        [TestCase("null")]
        public void ItHandlesReadingInvalidDecimalStringValuesCorrectly(string json)
        {
            // Arrange/Act
            var exception = Assert.Catch(() => JsonConvert.DeserializeObject<Decimal>(json, new DockerPercentStringConverter()));

            // Assert
            exception.Should().BeOfType<JsonSerializationException>();
        }


        [Test]
        public void ItHandlesReadingValidFloatValuesCorrectly()
        {
            // Arrange/Act
            var result = JsonConvert.DeserializeObject<Decimal>("1.99", new DockerPercentStringConverter());

            // Assert
            result.Should().Be(1.99m);
        }

        [Test]
        public void ItHandlesReadingValidIntegerValuesCorrectly()
        {
            // Arrange/Act
            var result = JsonConvert.DeserializeObject<Decimal>("1", new DockerPercentStringConverter());

            // Assert
            result.Should().Be(1m);
        }

        [Test]
        [TestCase("null")]
        [TestCase("[\"thisIs\",\"AnArray\"]")]
        public void ItWillThrowIfAskedToHandleAnUnexpectedDataType(string json)
        {
            // Arrange/Act
            var exception = Assert.Catch(() => JsonConvert.DeserializeObject<Decimal>(json, new DockerPercentStringConverter()));

            // Assert
            exception.Should().BeOfType<JsonSerializationException>();
        }

        [Test]
        public void ItWillThrowIfWriteJsonIsCalled()
        {
            // Arrange
            var sut = new DockerPercentStringConverter();
            
            // Act
            var exception = Assert.Catch(() => sut.WriteJson(new JsonTextWriter(TextWriter.Null), 32m, JsonSerializer.CreateDefault()));
            
            // Assert
            exception.Should().BeOfType<NotImplementedException>();
        }

    }
}
