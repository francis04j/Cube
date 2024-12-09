using CubeLogic.TransactionsConverter.Errors;
using CubeLogic.TransactionsConverter.FileValidators;
using FluentAssertions;

namespace CubeLogic.TransactionsConverter.UnitTests.FileValidators;

public class JsonFileValidatorTests
{
    readonly IFileValidator _fileValidator;
    
    public JsonFileValidatorTests()
    {
        _fileValidator = new JsonFileValidator();
    }
    
    [Fact]
    public void Validate_ShouldFail_WhenFilepathIsNullOrEmpty()
    {
        //Arrange
        var filepath = string.Empty;
        
        //Act
        var result = _fileValidator.ValidateFile(filepath);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Message.Should().Be($"Given {filepath} is null or empty.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenFileContentIsNotValidJson()
    {
        //Arrange
        var fileContent = "Hello World";
        var filepath = "test.json";
        
        File.WriteAllText(filepath, fileContent);
        
        //Act
        var result = _fileValidator.ValidateFile(filepath);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Metadata["Code"].Should().Be(ErrorCode.InvalidJson.ToString());
        
        //cleanup
        File.Delete(filepath);
    }

    [Fact]
    public void Validate_ShouldSuccess_WhenFileContentIsValidJson()
    {
        var fileContent = @"{""Timezone"": ""Europe/Malta""}";
        var filepath = "test.json";
        
        File.WriteAllText(filepath, fileContent);
        
        //Act
        var result = _fileValidator.ValidateFile(filepath);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        
        //cleanup
        File.Delete(filepath);
    }
}