using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Common.Models.User
{
    public class Register
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 30 characters.")]
        [DisplayName("Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Email must be between 6 and 30 characters.")]
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DisplayName("Password")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Required(ErrorMessage = "Confirm Password is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
