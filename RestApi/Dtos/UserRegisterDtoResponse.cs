using System.ComponentModel.DataAnnotations;
using RestApi.CustomValid;

namespace RestApi.Dtos
{
    public class UserRegisterDtoResponse
    {
        // add id
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RoleValid(ErrorMessage = "Role must be either 'Product', 'Note', or both separated by a comma")]
        public string Role { get; set; } = string.Empty;
    }
}