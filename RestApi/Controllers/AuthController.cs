using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using RestApi.Data;
using RestApi.Models;
using RestApi.Models.Dto;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace RestApi.Controllers
{
    [EnableCors("_myAllowSpecificOrigins")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        string name = "";
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
        

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }


        [HttpPost("login")]
        public IActionResult Login(UserLoginDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Password = userDto.Password
            };
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                // Check if the password is correct
                if (BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password))
                {
                    Console.WriteLine("Password is correct");
                }
                else
                {
                    return Unauthorized("Invalid password");
                }
            }
            else
            {
                return NotFound("User not found");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var JwtKey = _configuration.GetValue<string>("Jwt:Key") ?? "";
            var key = Encoding.ASCII.GetBytes(JwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, existingUser.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            if (tokenDescriptor != null)
            {
                ParseRole(existingUser.Role, tokenDescriptor);
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            existingUser.Password = null; // Remove password from the response
            return Ok(new { Token = tokenString, User = existingUser });
        }

        private void ParseRole(string roles, SecurityTokenDescriptor tokenDescriptor)
        {
            var roleList = roles.Split(',').Select(r => r.Trim()).ToList();
            foreach (var role in roleList)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

    }
}