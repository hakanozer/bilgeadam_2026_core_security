using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using RestApi.Data;
using RestApi.Dtos;
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
        private readonly IMapper _mapper;
        private readonly AuthService _authService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IMapper mapper, AuthService authService)
        {
            _configuration = configuration;
            _context = context;
            _authService = authService;
            _mapper = mapper;
        }
        

        [HttpPost("register")]
        public IActionResult Register(UserRegisterDto userRegisterDto)
        {
            var user = _mapper.Map<User>(userRegisterDto);
        
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            var userResponse = _mapper.Map<UserRegisterDtoResponse>(user);
            return Ok(userResponse);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var result = await _authService.LoginAsync(userDto);

            if (!result.IsSuccess)
            {
                if (result.Message == "User not found")
                    return NotFound(result.Message);

                return Unauthorized(result.Message);
            }

            // Cookie burada set edilir (HTTP concern)
            Response.Cookies.Append("jwt", result.Message, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });
            return Ok(result.User);
        }


        [HttpPost("loginUser")]
        public async Task<IActionResult> LoginUser(UserLoginDto userDto)
        {
            var result = await _authService.LoginAsyncUser(userDto);

            if (!result.IsSuccess)
            {
                if (result.Message == "User not found")
                    return NotFound(result.Message);

                return Unauthorized(result.Message);
            }

            return Ok(result.User);
        }

    }
}
