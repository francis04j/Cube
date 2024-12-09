using CubeLogic.TransactionsConverter.Configuration;
using CubeLogic.TransactionsConverter.Entities;
using CubeLogic.TransactionsConverter.Errors;
using CubeLogic.TransactionsConverter.FileValidators;
using FluentAssertions;
using FluentResults;
using Moq;

namespace CubeLogic.TransactionsConverter.UnitTests.Configuration;

public class ConfigServiceTests
{
    readonly Mock<IFileValidator> _fileValidator;
    readonly Mock<IJsonDeserializer> _jsonDeserializer;
    readonly ConfigService _configService;
    Config config;

    public ConfigServiceTests()
    {
        _fileValidator = new Mock<IFileValidator>();
        _fileValidator.Setup(x => x.ValidateFile(It.IsAny<string>())).Returns(Result.Ok);
        _jsonDeserializer = new Mock<IJsonDeserializer>();
        config = new Config("Europe/London", new List<Instrument>());
        _jsonDeserializer.Setup(x => x.Deserialize<Config>(It.IsAny<string>())).Returns(config);
        _configService = new ConfigService(_fileValidator.Object, _jsonDeserializer.Object);
    }

    [Fact]
    public void LoadConfig_Should_ReturnError_WhenValidationFails()
    {
        //Arrange
        _fileValidator.Setup(x => x.ValidateFile(It.IsAny<string>())).
            Returns(Result.Fail(ErrorFactory.Create(ErrorCode.InvalidJson, "Invalid JSON")));
        
        //Act
        var result = _configService.LoadConfig("filepath");
        
        //Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Metadata["Code"].Should().Be(ErrorCode.InvalidJson.ToString());
    }
    
    [Fact]
    public void LoadConfig_Should_ReturnError_WhenJsonDeserializationFails()
    {
        //Arrange
      _jsonDeserializer.Setup(x => x.Deserialize<Config>(It.IsAny<string>()))
          .Returns(new Config("", new List<Instrument>()));
      string filepath = "random.json";
      File.WriteAllText(filepath, "random");
      
        //Act
        var result = _configService.LoadConfig(filepath);
        
        //Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Metadata["Code"].Should().Be(ErrorCode.NullOrEmptyInput.ToString());
        
        //cleanup
        File.Delete(filepath);
    }
    
    [Fact]
    public void LoadConfig_Should_ReturnError_WhenUnexpectedErrorOccurs()
    {
        //Arrange
        _jsonDeserializer.Setup(x => x.Deserialize<Config>(It.IsAny<string>()))
            .Throws<Exception>();
        string filepath = "random.json";
        File.WriteAllText(filepath, "random");
      
        //Act
        var result = _configService.LoadConfig(filepath);
        
        //Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Metadata["Code"].Should().Be(ErrorCode.UnexpectedError.ToString());
        
        //cleanup
        File.Delete(filepath);
    }
    
    [Fact]
    public void LoadConfig_Should_ReturnConfig()
    {
        //Arrange
        string filepath = "random.json";
        File.WriteAllText(filepath, @"{""Timezone"": ""Europe/Malta""}");
      
        //Act
        var result = _configService.LoadConfig(filepath);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(config);
        
        //cleanup
        File.Delete(filepath);
    }
}