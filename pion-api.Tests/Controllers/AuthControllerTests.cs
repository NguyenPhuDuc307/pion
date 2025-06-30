using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using pion_api.Controllers;
using pion_api.Dtos;
using pion_api.Models;
using pion_api.Services;
using System.Threading.Tasks;
using Xunit;

namespace pion_api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            _jwtServiceMock = new Mock<IJwtService>();
            _controller = new AuthController(_userManagerMock.Object, _jwtServiceMock.Object);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var user = new ApplicationUser { FullName = "Test User", Email = "test@example.com" };
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("token");

            var result = await _controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal("Test User", response.FullName);
            Assert.Equal("test@example.com", response.Email);
            Assert.Equal("token", response.Token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            var loginDto = new LoginDto { Email = "wrong@example.com", Password = "wrong" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.Login(loginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            var registerDto = new RegisterDto { FullName = "Test User", Email = "test@example.com", Password = "password" };
            var user = new ApplicationUser { FullName = registerDto.FullName, Email = registerDto.Email };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password)).ReturnsAsync(IdentityResult.Success);
            _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).Returns("token");

            var result = await _controller.Register(registerDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal("Test User", response.FullName);
            Assert.Equal("test@example.com", response.Email);
            Assert.Equal("token", response.Token);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            var registerDto = new RegisterDto { FullName = "Test User", Email = "test@example.com", Password = "password" };
            var errors = new[] { new IdentityError { Description = "Error" } };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password)).ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _controller.Register(registerDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }
    }
}
