using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pion_api.Dtos;
using pion_api.Models;
using pion_api.Services;

namespace pion_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var token = _jwtService.GenerateToken(user);
            return Ok(new AuthResponseDto
            {
                FullName = user.FullName!,
                Email = user.Email!,
                Token = token
            });
        }

        return Unauthorized("Invalid credentials");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var token = _jwtService.GenerateToken(user);
            return Ok(new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Token = token
            });
        }

        return BadRequest(result.Errors);
    }
}