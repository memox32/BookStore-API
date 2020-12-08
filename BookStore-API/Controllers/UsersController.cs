using BookStore_API.Contracts;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _config;

    public UsersController(SignInManager<IdentityUser> signInManager, 
      UserManager<IdentityUser> userManager, 
      ILoggerService logger,
      IConfiguration config)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _logger = logger;
      _config = config;
    }

    /// <summary>
    /// User Login endpoint
    /// </summary>
    /// <param name="userDTO"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
    {
      var username = userDTO.UserName;
      var password = userDTO.PassWord;
      var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
    
      if(result.Succeeded)
      {
        var user = await _userManager.FindByNameAsync(username);
        var tokenString = await GenerateJSONWebToken(user);
        return Ok(new { token = tokenString });
      }

      return Unauthorized(userDTO);
    }

    private async Task<string> GenerateJSONWebToken(IdentityUser user)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
      var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
      };
      var roles = await _userManager.GetRolesAsync(user);
      claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

      var token = new JwtSecurityToken(_config["JWT:Issuer"],
        _config["JWT:Issuer"],
        claims,
        null,
        expires: DateTime.Now.AddMinutes(5),
        signingCredentials: credentials);
      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
