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

namespace pion_api.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IStorageService> _storageServiceMock;
        private readonly ProductsController _controller;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ProductsControllerTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(_options);
            _storageServiceMock = new Mock<IStorageService>();
            _controller = new ProductsController(_context, _storageServiceMock.Object);
            _context.Products.Add(new Product { Id = 1, Name = "Product 1", Tags = new List<string> { "tag1" }, ImageUrl = "uploads/old.jpg" });
            _context.Products.Add(new Product { Id = 2, Name = "Product 2", Tags = new List<string> { "tag2" } });
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetProducts_ReturnsAll()
        {
            var result = await _controller.GetProducts();
            Assert.Equal(2, (result.Value as IEnumerable<Product>)!.Count());
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_AndNotFound()
        {
            var ok = await _controller.GetProduct(1);
            Assert.Equal("Product 1", ok.Value!.Name);
            var notfound = await _controller.GetProduct(999);
            Assert.IsType<NotFoundResult>(notfound.Result);
        }

        [Fact]
        public async Task PostProduct_CreatesProduct_WithAndWithoutFile()
        {
            var dto = new ProductDto { Name = "New", Tags = "[\"t\"]" };
            var result = await _controller.PostProduct(dto);
            Assert.Equal("New", ((Product)((CreatedAtActionResult)result.Result!).Value!).Name);
            // With file
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(new System.IO.MemoryStream(new byte[] { 1 }));
            fileMock.Setup(f => f.ContentDisposition).Returns("form-data; name=\"file\"; filename=\"test.jpg\"");
            _storageServiceMock.Setup(s => s.SaveFileAsync(It.IsAny<System.IO.Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            var dto2 = new ProductDto { Name = "WithFile", Tags = "[\"t2\"]", File = fileMock.Object };
            var result2 = await _controller.PostProduct(dto2);
            Assert.Equal("WithFile", ((Product)((CreatedAtActionResult)result2.Result!).Value!).Name);
        }

        [Fact]
        public async Task PutProduct_UpdatesProduct_AndNotFound()
        {
            var dto = new ProductDto { Name = "Updated", Tags = "[\"t3\"]" };
            var result = await _controller.PutProduct(1, dto);
            Assert.IsType<NoContentResult>(result);
            var notfound = await _controller.PutProduct(999, dto);
            Assert.IsType<NotFoundResult>(notfound);
        }

        [Fact]
        public async Task PutProduct_Updates_WithFile_DeletesOldFile()
        {
            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(new System.IO.MemoryStream(new byte[] { 2 }));
            fileMock.Setup(f => f.ContentDisposition).Returns("form-data; name=\"file\"; filename=\"new.jpg\"");
            _storageServiceMock.Setup(s => s.DeleteFileAsync("old.jpg")).Returns(Task.CompletedTask).Verifiable();
            _storageServiceMock.Setup(s => s.SaveFileAsync(It.IsAny<System.IO.Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();
            var dto = new ProductDto { Name = "UpdateFile", Tags = "[\"t4\"]", File = fileMock.Object };
            var result = await _controller.PutProduct(1, dto);
            Assert.IsType<NoContentResult>(result);
            _storageServiceMock.Verify(s => s.DeleteFileAsync("old.jpg"), Times.Once);
            _storageServiceMock.Verify(s => s.SaveFileAsync(It.IsAny<System.IO.Stream>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct_AndNotFound_AndRemovesImage()
        {
            // Remove with image
            _storageServiceMock.Setup(s => s.DeleteFileAsync("uploads/old.jpg")).Returns(Task.CompletedTask).Verifiable();
            var result = await _controller.DeleteProduct(1);
            Assert.IsType<NoContentResult>(result);
            _storageServiceMock.Verify(s => s.DeleteFileAsync("uploads/old.jpg"), Times.Once);
            // Remove not found
            var notfound = await _controller.DeleteProduct(999);
            Assert.IsType<NotFoundResult>(notfound);
        }

        [Fact]
        public async Task SearchProducts_AllCases()
        {
            var found = await _controller.SearchProducts("Product 1");
            Assert.Single(found.Value!);
            var notfound = await _controller.SearchProducts("NotExist");
            Assert.Empty(notfound.Value!);
            var all = await _controller.SearchProducts("");
            Assert.Equal(2, all.Value!.Count());
        }

        // Context phụ để test concurrency exception
        private class TestDbContext : ApplicationDbContext
        {
            public TestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
            public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                throw new DbUpdateConcurrencyException();
            }
        }

        [Fact]
        public async Task PutProduct_Concurrency_ThrowsOrNotFound()
        {
            // Throw khi product còn
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var context = new TestDbContext(options);
            context.Products.Add(new Product { Id = 30, Name = "Old3", Tags = new List<string> { "t" } });
            context.SaveChanges();
            var controller = new ProductsController(context, _storageServiceMock.Object);
            var dto = new ProductDto { Name = "Update", Tags = "[\"t4\"]" };
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller.PutProduct(30, dto));
            // NotFound khi product không còn
            var context2 = new TestDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()).Options);
            var controller2 = new ProductsController(context2, _storageServiceMock.Object);
            var result = await controller2.PutProduct(12345, dto);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
