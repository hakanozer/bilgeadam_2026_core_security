using System.ComponentModel.DataAnnotations;
using RestApi.CustomValid;

namespace RestApi.Dtos
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        // 1 Özel karakter, 1 büyük harf, 1 küçük harf ve 1 rakam içermeli
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,15}$", ErrorMessage = "Password must contain at least one special character, one uppercase letter, one lowercase letter, and one number")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 15 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RoleValid(ErrorMessage = "Role must be either 'Product', 'Note', or both separated by a comma")]
        public string Role { get; set; } = string.Empty;
    }
}