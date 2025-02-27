using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Wmi.Api.Services;
using Wmi.Api.Models;
using Wmi.Api.Models.Dto;
using Wmi.Api.Data;

public class ProductServiceTests
{
    private readonly Mock<IDataRepository> _dataRepositoryMock;
    private readonly Mock<IBuyerService> _buyerServiceMock;
    private readonly Mock<IValidator<Product>> _validatorMock;
    private readonly Mock<INotify> _notifyMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _dataRepositoryMock = new Mock<IDataRepository>();
        _buyerServiceMock = new Mock<IBuyerService>();
        _validatorMock = new Mock<IValidator<Product>>();
        _notifyMock = new Mock<INotify>();
        _mapperMock = new Mock<IMapper>();

        _productService = new ProductService(
            _dataRepositoryMock.Object, 
            _buyerServiceMock.Object, 
            _validatorMock.Object, 
            _notifyMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Sku = "123", Title = "Product A", BuyerId = "b1"},
            new Product { Sku = "456", Title = "Product B", BuyerId = "b2"}
        };

        _dataRepositoryMock.Setup(repo => repo.GetProductsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetProductsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldFail_WhenSkuExists()
    {
        // Arrange
        var productDto = new CreateProductDto { Sku = "123", BuyerId = "B1", Title = "title"};

        _dataRepositoryMock.Setup(repo => repo.ExistsProductBySkuAsync(productDto.Sku))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.CreateProductAsync(productDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Sku already exists", result.Error);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldFail_WhenBuyerDoesNotExist()
    {
        // Arrange
        var productDto = new CreateProductDto { Sku ="123", BuyerId = "B1", Title = "title"};

        _dataRepositoryMock.Setup(repo => repo.ExistsProductBySkuAsync(productDto.Sku))
            .ReturnsAsync(false);
        _buyerServiceMock.Setup(s => s.ExistsBuyerAsync(productDto.BuyerId))
            .ReturnsAsync(Result<bool>.Fail("Buyer not found"));

        // Act
        var result = await _productService.CreateProductAsync(productDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("buyerId is invalid", result.Error);
    }

    [Fact]
    public async Task ChangeActiveStatusAsync_ShouldDeactivateProduct()
    {
        // Arrange
        var product = new Product { Sku = "123", Active = true, BuyerId = "B1", Title = "title"};

        _dataRepositoryMock.Setup(repo => repo.GetProductBySkuAsync("123"))
            .ReturnsAsync(product);
        _dataRepositoryMock.Setup(repo => repo.UpdateProductAsync(product))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.ChangeActiveStatusAsync("123", false);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Value.Active);
        _notifyMock.Verify(n => n.Notify(product.BuyerId, It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task ChangeActiveStatusAsync_ShouldNotify_WhenProductIsDeactivated()
    {
        // Arrange
        var product = new Product { Sku = "123", Active = true, BuyerId = "B1", Title = "title"};

        _dataRepositoryMock.Setup(repo => repo.GetProductBySkuAsync("123")).ReturnsAsync(product);
        _dataRepositoryMock.Setup(repo => repo.UpdateProductAsync(product)).ReturnsAsync(true);

        // Act
        var result = await _productService.ChangeActiveStatusAsync("123", false);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Value.Active);
        _notifyMock.Verify(n => n.Notify(product.BuyerId, It.Is<string>(msg => msg.Contains("deactivated"))), Times.Once);
    }

    [Fact]
    public async Task ChangeBuyerAsync_ShouldNotify_BothOldAndNewBuyer()
    {
        // Arrange
        var product = new Product { Sku = "123", BuyerId = "OldBuyer", Title = "title"};

        _buyerServiceMock.Setup(s => s.ExistsBuyerAsync("NewBuyer")).ReturnsAsync(Result<bool>.Ok(true));
        _dataRepositoryMock.Setup(repo => repo.GetProductBySkuAsync("123")).ReturnsAsync(product);
        _dataRepositoryMock.Setup(repo => repo.UpdateProductAsync(product)).ReturnsAsync(true);

        // Act
        var result = await _productService.ChangeBuyerAsync("123", "NewBuyer");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("NewBuyer", result.Value.BuyerId);
        _notifyMock.Verify(n => n.Notify("OldBuyer", It.Is<string>(msg => msg.Contains("unassigned"))), Times.Once);
        _notifyMock.Verify(n => n.Notify("NewBuyer", It.Is<string>(msg => msg.Contains("assigned"))), Times.Once);
    }
    
    public async Task ChangeBuyerAsync_ShouldFail_WhenSkuNotFound()
    {
        // Arrange
        _dataRepositoryMock.Setup(repo => repo.GetProductBySkuAsync("999"))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.ChangeBuyerAsync("999", "B2");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Sku is out of range", result.Error);
    }
}
