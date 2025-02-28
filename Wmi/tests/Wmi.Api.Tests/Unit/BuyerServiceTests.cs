
using AutoMapper;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Wmi.Api.Data;
using Wmi.Api.Services; // Adjust namespace as needed
using Wmi.Api.Models;
using Wmi.Api.Models.Dto; // Adjust namespace for Buyer, Result, etc.

public class BuyerServiceTests
{
    private readonly Mock<IDataRepository> _dataRepositoryMock;
    private readonly Mock<IValidator<Buyer>> _validatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly BuyerService _buyerService;

    public BuyerServiceTests()
    {
        _dataRepositoryMock = new Mock<IDataRepository>();
        _validatorMock = new Mock<IValidator<Buyer>>();
        _mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<BuyerService>>();
        _buyerService = new BuyerService(loggerMock.Object, _dataRepositoryMock.Object, _validatorMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetBuyersAsync_ShouldReturnBuyers()
    {
        // Arrange
        var buyers = new List<Buyer>
        {
            new Buyer { Id = "1", Name = "John Doe", Email = "john@example.com" },
            new Buyer { Id = "2", Name = "Jane Doe", Email = "jane@example.com" }
        };

        _dataRepositoryMock.Setup(repo => repo.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(buyers);

        // Act
        var result = await _buyerService.GetBuyersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task CreateBuyerAsync_ShouldReturnFailure_WhenEmailExists()
    {
        // Arrange
        var buyerDto = new CreateBuyerDto { Name = "John Doe", Email = "john@example.com" };

        _dataRepositoryMock.Setup(repo => repo.ExistsBuyerByEmailAsync(buyerDto.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _buyerService.CreateBuyerAsync(buyerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.Error);
    }

    [Fact]
    public async Task CreateBuyerAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var buyerDto = new CreateBuyerDto { Name = "", Email = "invalidemail" };
        var buyer = new Buyer() { Id = "1", Name = "", Email = "invalidemail" };

        _dataRepositoryMock.Setup(repo => repo.ExistsBuyerByEmailAsync(buyerDto.Email))
            .ReturnsAsync(false);

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Buyer>(), default))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Invalid email format"),
                new ValidationFailure("Name", "Name is required")
            }));
        
        _mapperMock
            .Setup(m => m.Map<Buyer>(buyerDto))
            .Returns(buyer);

        // Act
        var result = await _buyerService.CreateBuyerAsync(buyerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid email format", result.Error);
        Assert.Contains("Name is required", result.Error);
    }

    [Fact]
    public async Task ExistsBuyerAsync_ShouldReturnTrue_WhenBuyerExists()
    {
        // Arrange
        _dataRepositoryMock.Setup(repo => repo.ExistsBuyerAsync("1"))
            .ReturnsAsync(true);

        // Act
        var result = await _buyerService.ExistsBuyerAsync("1");

        // Assert
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateBuyerAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var updateBuyerDto = new UpdateBuyerDto { Name = "", Email = "invalidemail" };
        var buyer = new Buyer() { Id = "1", Name = "", Email = "invalidemail" };
        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Buyer>(), default))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Invalid email format"),
                new ValidationFailure("Name", "Name is required")
            }));
        
        _mapperMock
            .Setup(m => m.Map<Buyer>(updateBuyerDto))
            .Returns(buyer);

        // Act
        var result = await _buyerService.UpdateBuyerAsync("1", updateBuyerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid email format", result.Error);
        Assert.Contains("Name is required", result.Error);
    }

    [Fact]
    public async Task DeleteBuyerAsync_ShouldReturnFailure_WhenBuyerDoesNotExist()
    {
        // Arrange
        _dataRepositoryMock.Setup(repo => repo.ExistsBuyerAsync("1"))
            .ReturnsAsync(false);

        // Act
        var result = await _buyerService.DeleteBuyerAsync("1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Buyer not found", result.Error);
    }

    [Fact]
    public async Task DeleteBuyerAsync_ShouldReturnSuccess_WhenDeletionSucceeds()
    {
        // Arrange
        _dataRepositoryMock.Setup(repo => repo.ExistsBuyerAsync("911"))
            .ReturnsAsync(true);

        _dataRepositoryMock.Setup(repo => repo.ExistsProductWithBuyerIdAsync("911"))
            .ReturnsAsync(false);

        _dataRepositoryMock.Setup(repo => repo.DeleteBuyerAsync("911"))
            .ReturnsAsync(true);

        // Act
        var result = await _buyerService.DeleteBuyerAsync("911");

        // Assert
        Assert.True(result.Success);
    }
}
