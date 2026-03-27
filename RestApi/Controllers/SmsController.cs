using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RestApi.Data;
using RestApi.Models;
using SQLitePCL;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Product,Note")]
    public class SmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Sms()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var salt = Guid.NewGuid().ToString();
            var smsCode = GenerateCode();
            var sms = new Sms
            {
                UserId = int.Parse(id),
                SmsKey = Sha256(smsCode, salt),
                Salt = salt
            };
            _context.Sms.Add(sms);
            _context.SaveChanges();
            var response = new
            {
                SmsCode = smsCode
            };
            return Ok(response);
        }

        [HttpPost]
        public IActionResult Sms(SmsVerifyRequest smsRequest)
        {   
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var sms = _context.Sms.FirstOrDefault(s => s.UserId == int.Parse(id));
            if (sms == null)            {
                return NotFound("SMS not found.");
            }
            var hashedCode = Sha256(smsRequest.SmsKey, sms.Salt);
            if (hashedCode == sms.SmsKey){
                _context.Sms.Remove(sms);
                _context.SaveChanges();
                return Ok("SMS verified successfully.");
            }
            return BadRequest("Invalid SMS code.");
        }

        private string GenerateCode()
        {
            var number = RandomNumberGenerator.GetInt32(10000, 100000);
            return number.ToString();
        }

        private string Sha256(string value, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = value + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        
    }
}
