using System.Diagnostics;
using System.Globalization;
using CubeLogic.TransactionsConverter.Processors;
using FluentAssertions;
using Moq;
using Bogus;
using CubeLogic.TransactionsConverter.CustomCsvReader;
using CubeLogic.TransactionsConverter.CsvWriter;
using CubeLogic.TransactionsConverter.Entities;
using CubeLogic.TransactionsConverter.Errors;
using CubeLogic.TransactionsConverter.TimezoneConverter;
using FluentResults;
using Microsoft.Extensions.Logging;

//using Instrument = System.Diagnostics.Metrics.Instrument;

namespace CubeLogic.TransactionsConverter.UnitTests.Processors;

public class TransactionProcessorTests
{
    readonly Faker _faker;
    readonly Mock<IUtCconverter> _utCconverter;
    readonly Mock<ICsvReaderFactory> _csvReaderFactory;
    readonly Mock<ICsvWriterFactory> _csvWriterFactory;
    Mock<ICsvReader> mockCsvReader;
    Mock<ICsvWriter> mockCsvWriter;
    Mock<ILogger<TransactionProcessor>> mockLogger;
    
    readonly TransactionProcessor _transactionProcessor;
    private Config _validConfig;
    
    public TransactionProcessorTests()
    {
        _faker = new Faker();
        _utCconverter = new();
        _csvReaderFactory = new Mock<ICsvReaderFactory>();
        _csvWriterFactory = new Mock<ICsvWriterFactory>();
        SetupCsvReader(_csvReaderFactory);
        SetupCsvWriter(_csvWriterFactory);
        mockLogger = new Mock<ILogger<TransactionProcessor>>();
        _transactionProcessor = new TransactionProcessor(_utCconverter.Object, _csvReaderFactory.Object, _csvWriterFactory.Object, mockLogger.Object);
        _validConfig = new( "Europe/London", new List<Instrument>
        {
            new(1, "StockA",  "USA" ),
            new (  2, "StockB", "Canada" )
        });
        _utCconverter.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Ok<string>("2023-11-10T03:02:08.6671230Z"));
    }

    private void SetupCsvReader(Mock<ICsvReaderFactory> mockCsvReaderFactory)
    {
        mockCsvReader = new Mock<ICsvReader>();
        mockCsvReader.SetupSequence(r => r.Read())
            .Returns(true) // Read header
            .Returns(true) // Read first record
            .Returns(false); // No more records

        mockCsvReader.Setup(r => r.ReadHeader()).Returns(true);
        mockCsvReader.Setup(r => r.GetRecords<InputTransaction>()).Returns(new List<InputTransaction>
        {
            new InputTransaction
            (
                "1",
                "AddOrder",
                 "2024-12-01T10:00:00",
                 100.5m,
                 1
            )
        });

        mockCsvReaderFactory.Setup(f => f.CreateCsvReader(It.IsAny<string>())).Returns(mockCsvReader.Object);
    }

    private void SetupCsvWriter(Mock<ICsvWriterFactory> mockCsvWriterFactory)
    {
        mockCsvWriter = new Mock<ICsvWriter>();

        mockCsvWriter.Setup(x => x.WriteHeader<InputTransaction>());
        mockCsvWriter.Setup(x => x.WriteRecord(It.IsAny<InputTransaction>()));
        mockCsvWriter.Setup(x => x.NextRecord());
        
        mockCsvWriterFactory.Setup(f => f.CreateCsvWriter(It.IsAny<string>())).Returns(mockCsvWriter.Object);
    }

    [Fact]
    public void ProcessTransactions_ShouldReturnError_WhenInputFilePathIsEmpty()
    {
        //Arrange
        string mockFilePath = string.Empty;
        var config = new Config("Timezone", new List<Instrument>());
        
        //Act
        var result = _transactionProcessor.ProcessTransactions(mockFilePath, mockFilePath, config);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Message.Should().Be($"Given inputPath: {mockFilePath}, cannot be null or empty");
        error.Metadata["Code"].Should().Be(ErrorCode.NullOrEmptyInput.ToString());
    }
    
    [Fact]
    public void ProcessTransactions_ShouldReturnError_WhenInputFilePathIsNull()
    {
        //Arrange
        string mockFilePath = null!;
        var config = new Config("Timezone", new List<Instrument>());
        
        //Act
        var result = _transactionProcessor.ProcessTransactions(mockFilePath, mockFilePath, config);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Message.Should().Be($"Given inputPath: {mockFilePath}, cannot be null or empty");
        error.Metadata["Code"].Should().Be(ErrorCode.NullOrEmptyInput.ToString());
    }
    
    [Fact]
    public void ProcessTransactions_ShouldReturnError_WhenOutputFilePathIsEmpty()
    {
        //Arrange
        string mockFilePath = string.Empty;
        var config = new Config("Timezone", new List<Instrument>());
        
        //Act
        var result = _transactionProcessor.ProcessTransactions("mockinputFilePath", mockFilePath, config);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Message.Should().Be($"Given outputPath: {mockFilePath}, cannot be null or empty");
        error.Metadata["Code"].Should().Be(ErrorCode.NullOrEmptyInput.ToString());
    }
    
    [Fact]
    public void ProcessTransactions_ShouldReturnError_WhenOutputFilePathIsNull()
    {
        //Arrange
        string mockFilePath = null!;
        var config = new Config("Timezone", new List<Instrument>()!);
        
        //Act
        var result = _transactionProcessor.ProcessTransactions("mockinputFilePath", mockFilePath, config);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Message.Should().Be($"Given outputPath: {mockFilePath}, cannot be null or empty");
        error.Metadata["Code"].Should().Be(ErrorCode.NullOrEmptyInput.ToString());
    }
    
    [Fact]
    public void ProcessTransactions_ShouldReturnError_WhenThereIsNoTimezoneInConfig()
    {
        //Arrange
        string mockFilePath = _faker.System.FilePath();
        var configWithNoInstrument = new Config("", null!);
        
        //Act
        var result = _transactionProcessor.ProcessTransactions(mockFilePath, mockFilePath, configWithNoInstrument);
        
        //Assert
        result.IsSuccess.Should().BeFalse();
        var error = result.Errors.First();
        error.Message.Should().Be("No timezone specified, check if config.json file is correct");
        error.Metadata["Code"].Should().Be(ErrorCode.NoTimezoneSpecified.ToString());
    }

    [Fact]
    public void ProcessTransactions_ShouldUseUtcConverter_WhenRecordHasDateTimeAndTimezone()
    {
        //Arrange
        _utCconverter.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>()));
        
        //Act
        _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), _validConfig);
        
        //Assert
        _utCconverter.Verify(x => x.Convert(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    
     [Fact]
     public void ProcessTransactions_ShouldUFail_WhenRecordHasDateTimeThatIsInvalid()
     {
        //Arrange
        string mockFilePath = _faker.System.FilePath();
        _utCconverter.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Fail<string>(ErrorFactory.Create(ErrorCode.UnexpectedError, "Failed")));
    
        //Act
        var result = _transactionProcessor.ProcessTransactions(mockFilePath, mockFilePath, _validConfig);
    
         //Assert
         result.IsSuccess.Should().BeFalse();
         result.Errors.Should().HaveCount(1);
         result.Errors[0].Metadata["Code"].Should().Be(ErrorCode.InvalidDateTime.ToString());
     }
    
    [Fact]
    public void ProcessTransactions_ShouldHandleValidInputCorrectly()
    {
        // Arrange
        string orderId = "1";
        string addPrice = "1.00";
        string updatePrice = "2.00";
      
        mockCsvReader.Setup(r => r.ReadHeader()).Returns(true);
        mockCsvReader.Setup(r => r.GetRecords<InputTransaction>()).Returns(new List<InputTransaction>
        {
            new InputTransaction
            (
                orderId,
                "AddOrder",
                "2024-12-01T10:00:00",
                decimal.Parse(addPrice),
                1
            ),
            new InputTransaction
            (
                orderId,
                "UpdateOrder",
                "2024-12-01T11:00:00",
                decimal.Parse(updatePrice),
                1
            )
        });

        _csvReaderFactory.Setup(f => f.CreateCsvReader(It.IsAny<string>())).Returns(mockCsvReader.Object);
        
        // Act
        var result = _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), _validConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "ADD" && x.OrderId == orderId && x.Price.ToString() == addPrice)), Times.Exactly(1));
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "UPDATE" && x.OrderId == orderId && x.Price.ToString() == updatePrice)), Times.Exactly(1));

    }

    [Fact]
    public void ProcessTransactions_ShouldSkipDuplicateUpdates_WhenSameOrderAndSamePrice()
    {
        // Arrange
        string orderId = "1";
        string addPrice = "1.00";
        string updatePrice = "2.00";
      
        mockCsvReader.Setup(r => r.ReadHeader()).Returns(true);
        mockCsvReader.Setup(r => r.GetRecords<InputTransaction>()).Returns(new List<InputTransaction>
        {
            new InputTransaction
            (
                orderId,
                "AddOrder",
                "2024-12-01T10:00:00",
                decimal.Parse(addPrice),
                1
            ),
            new InputTransaction
            (
                orderId,
                "UpdateOrder",
                "2024-12-01T11:00:00",
                decimal.Parse(updatePrice),
                1
            ),
            new InputTransaction
            (
                orderId,
                "UpdateOrder",
                "2024-12-01T11:00:00",
                decimal.Parse(updatePrice),
                1
            )
        });

        _csvReaderFactory.Setup(f => f.CreateCsvReader(It.IsAny<string>())).Returns(mockCsvReader.Object);
        _utCconverter.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Ok<string>("2023-11-10T03:02:08.6671230Z"));
    
        // Act
        var result = _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), _validConfig);

        // Assert
        
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "ADD" && x.OrderId == orderId && x.Price.ToString(CultureInfo.InvariantCulture) == addPrice)), Times.Exactly(1));
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "UPDATE" && x.OrderId == orderId && x.Price.ToString(CultureInfo.InvariantCulture) == updatePrice)), Times.Exactly(1));
    }

    [Fact]
    public void ProcessTransactions_ShouldInsertError_WhenMissingInstrument()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>()
        );

        // Act
        var result = _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "ADD" && x.InstrumentName == "Error")), Times.Exactly(1));
    }
    
    [Fact]
    public void ProcessTransactions_ShouldLogError_WhenMissingInstrument()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>()
        );

        // Act
        var result = _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockCsvWriter.Verify(r => r.WriteRecord(It.Is<OutputTransaction>(x => 
            x.Type == "ADD" && x.InstrumentName == "Error")), Times.Exactly(1));
    }

        [Fact]
    public void ProcessTransactions_ShouldLogWarning_WhenInstrumentIdNotFound()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>()
        );

        // Act
        var result = _transactionProcessor.ProcessTransactions(_faker.System.FilePath(), _faker.System.FilePath(), config);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("InstrumentId 1 not found in config.json")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!
            ),
            Times.Once
        );
    }
    
}
