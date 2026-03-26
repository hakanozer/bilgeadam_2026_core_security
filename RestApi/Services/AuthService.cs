using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestApi.Data;
using RestApi.Models;
using RestApi.Models.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IMapper mapper)
    {
        _context = context;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<(bool IsSuccess, string Message, User? User)> LoginAsync(UserLoginDto userDto)
    {
        var user = _mapper.Map<User>(userDto);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username);

        if (existingUser == null)
            return (false, "User not found", null);

        if (!BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password))
            return (false, "Invalid password", null);

        // JWT oluştur
        var token = GenerateJwtToken(existingUser);

        // Cookie burada üretilmez ❗ (Controller işi)
        existingUser.Password = null;

        return (true, token, existingUser);
    }

    private string GenerateJwtToken(User existingUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var JwtKey = _configuration.GetValue<string>("Jwt:Key") ?? "";
        var key = Encoding.ASCII.GetBytes(JwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                new Claim(ClaimTypes.Name, existingUser.Username),
                new Claim(ClaimTypes.Gender, "Not Specified")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        ParseRole(existingUser.Role, tokenDescriptor);

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private void ParseRole(string roles, SecurityTokenDescriptor tokenDescriptor)
    {
        var roleList = roles.Split(',').Select(r => r.Trim()).ToList();
        foreach (var role in roleList)
        {
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }

    public async Task<(bool IsSuccess, string Message, User? User)> LoginAsyncUser(UserLoginDto userDto)
    {
        var user = _context.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Username = {userDto.Username} and Password = {userDto.Password}").FirstOrDefault();
        if (user == null)
            return (false, "User not found", null);
        return (true, "User logged in successfully", user);
    }
    
}