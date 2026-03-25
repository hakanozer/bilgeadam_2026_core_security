using System;
using System.ComponentModel.DataAnnotations;

namespace RestApi.CustomValid
{
    public class RoleValid : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string input = value.ToString().Trim();

            if (string.IsNullOrEmpty(input))
                return new ValidationResult("Role boş olamaz.");

            string[] validRoles = { "Product", "Note" };
            string[] roles = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (roles.Length == 0)
                return new ValidationResult("En az bir rol gereklidir.");

            if (roles.Length > 2)
                return new ValidationResult("En fazla iki rol belirtilebilir.");

            foreach (var role in roles)
            {
                string trimmedRole = role.Trim();
                if (Array.IndexOf(validRoles, trimmedRole) < 0)
                    return new ValidationResult($"'{trimmedRole}' geçersiz bir roldür. Geçerli roller: Product, Note");
            }

            return ValidationResult.Success;
        }
    }
}