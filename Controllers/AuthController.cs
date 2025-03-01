using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StrategyGame.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using StrategyGame.Requests;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Register user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            // Automatically sign in the user after successful registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new { Message = "User registered and logged in successfully" });
        }

        return BadRequest(result.Errors);
    }

    // Login user
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Successfully logged in, the identity cookie is automatically created by ASP.NET Core Identity
            return Ok(new { Message = "Login successful" });
        }

        return Unauthorized(new { Message = "Invalid login attempt." });
    }

    // Logout the user and delete the cookies
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync(); // Sign out the user on the server-side
        Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Clear the session cookie

        return Ok(new { Message = "Logged out successfully" });
    }

    [HttpGet("is-authenticated")]
    public IActionResult IsAuthenticated()
    {
        // Check if the user is authenticated based on the session cookie
        if (User.Identity.IsAuthenticated)
        {
            return Ok(new { message = "User is authenticated" });
        }

        return Unauthorized(new { message = "User is not authenticated" });
    }

}