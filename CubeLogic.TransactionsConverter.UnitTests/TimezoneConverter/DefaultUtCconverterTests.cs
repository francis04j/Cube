using CubeLogic.TransactionsConverter.TimezoneConverter;
using FluentAssertions;
using FluentAssertions.Extensions;
using FluentResults;

namespace CubeLogic.TransactionsConverter.UnitTests.TimezoneConverter;

public class DefaultUtCconverterTests
{
    private readonly DefaultUtCconverter _sut = new();

    [Fact]
    public void Convert_ShouldReturnErrorWhenDateTimeIsEmpty()
    {
        //Arrange
        string dateTime = String.Empty;

        //Act
        var result = _sut.Convert(dateTime, TimeZoneInfo.Utc.ToString());
        
        //Assert
        result.IsFailed.Should().BeTrue();
        var error = result.Errors.First();
        error.Message.Should().Be($"Record time: ${dateTime} is invalid");
    }
    
    [Fact]
    public void Convert_ShouldReturnErrorWhenDateTimeIsInvalidFormat()
    {
        //Arrange
        string dateTime = String.Empty;

        //Act
        var result = _sut.Convert(dateTime, TimeZoneInfo.Utc.ToString());
        
        //Assert
        result.IsFailed.Should().BeTrue();
        var error = result.Errors.First();
        error.Message.Should().Be($"Record time: ${dateTime} is invalid");
    }
    
    [Fact]
    public void Convert_ShouldReturnErrorWhenTimezoneIsInvalid()
    {
        //Arrange
        string dateTime = DateTime.Today.ToString("yyyyMMdd");
        string timezone = String.Empty;
        
        //Act
        var result = _sut.Convert(dateTime, timezone);
        
        //Assert
        result.IsFailed.Should().BeTrue();
        var error = result.Errors.First();
        error.Message.Should().NotBeEmpty();
    }
    
    [Fact]
    public void Convert_ShouldReturnErrorWhenTimezoneIsNotFound()
    {
        //Arrange
        string dateTime = DateTime.Today.ToString("yyyyMMdd");
        string timezone = "Timezone/Random";
        
        //Act
        var result = _sut.Convert(dateTime, timezone);
        
        //Assert
        result.IsFailed.Should().BeTrue();
        var error = result.Errors.First();
        error.Message.Should().NotBeEmpty();
    }
    
    [Fact]
    public void Convert_ShouldReturnUtcTimeWhenDateTimeAndTimezoneIsInvalid()
    {
        //Arrange
        string dateTime = "2023-11-10T01:02:03.6671230";
        string timezone = "Europe/Malta";
        
        //Act
        var result = _sut.Convert(dateTime, timezone);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("2023-11-10T00:02:03.6671230Z");
    }
}