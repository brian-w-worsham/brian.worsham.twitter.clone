using System.ComponentModel.DataAnnotations;

namespace worsham.twitter.clone.Utils
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        public int MinimumLength { get; set; }
        public int MaximumLength { get; set; }

        public PasswordValidationAttribute(int minimumLength, int maximumLength)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            if (password == null || password.Length < MinimumLength || password.Length > MaximumLength)
            {
                return new ValidationResult($"The password must be between {MinimumLength} and {MaximumLength} characters long.");
            }

            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("The password must contain at least one lowercase letter.");
            }

            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("The password must contain at least one uppercase letter.");
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ValidationResult("The password must contain at least one special character.");
            }

            return ValidationResult.Success;
        }
    }
}
