using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using pion_api.Controllers;
using pion_api.Data;
using pion_api.Dtos;
using pion_api.Models;
using pion_api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.InMemory;

namespace pion_api.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IStorageService> _storageServiceMock;
        private readonly ProductsController _controller;

        private ApplicationDbContext CreateMockDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        public ProductsControllerTests()
        {
            _context = CreateMockDbContext();
            _storageServiceMock = new Mock<IStorageService>();
            _controller = new ProductsController(_context, _storageServiceMock.Object);

            // Seed data
            _context.Products.Add(new Product { Id = 1, Name = "Product 1", Tags = new List<string> { "tag1" } });
            _context.Products.Add(new Product { Id = 2, Name = "Product 2", Tags = new List<string> { "tag2" } });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            var result = await _controller.GetProducts();
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Value);
            Assert.Equal(2, products.Count());
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenExists()
        {
            var result = await _controller.GetProduct(1);
            var product = Assert.IsType<Product>(result.Value);
            Assert.Equal("Product 1", product.Name);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenNotExists()
        {
            var result = await _controller.GetProduct(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct_WhenExists()
        {
            var result = await _controller.DeleteProduct(1);
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Products.FindAsync(1));
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenNotExists()
        {
            var result = await _controller.DeleteProduct(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostProduct_CreatesProduct()
        {
            var dto = new ProductDto { Name = "New Product", Tags = "[\"tag3\",\"tag4\"]" };
            var result = await _controller.PostProduct(dto);
            var created = Assert.IsType<ActionResult<Product>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
            var product = Assert.IsType<Product>(createdResult.Value);
            Assert.Equal("New Product", product.Name);
            Assert.Contains("tag3", product.Tags);
            Assert.Contains("tag4", product.Tags);
        }

        [Fact]
        public async Task PutProduct_UpdatesProduct()
        {
            var dto = new ProductDto { Name = "Updated Product", Tags = "[\"tag5\",\"tag6\"]" };
            var result = await _controller.PutProduct(2, dto);
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Products.FindAsync(2);
            Assert.Equal("Updated Product", updated!.Name);
            Assert.Contains("tag5", updated.Tags);
            Assert.Contains("tag6", updated.Tags);
        }

        [Fact]
        public async Task PutProduct_ReturnsNotFound_WhenNotExists()
        {
            var dto = new ProductDto { Name = "NotFound", Tags = "[\"tagX\"]" };
            var result = await _controller.PutProduct(999, dto);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostProduct_AddsProductToDb()
        {
            var dto = new ProductDto { Name = "Db Product", Tags = "[\"tagDb\"]" };
            await _controller.PostProduct(dto);
            var dbProduct = _context.Products.FirstOrDefault(p => p.Name == "Db Product");
            Assert.NotNull(dbProduct);
            Assert.Contains("tagDb", dbProduct!.Tags);
        }

        // Có thể bổ sung test cho các trường hợp đặc biệt hơn nếu cần thiết.
    }
}
