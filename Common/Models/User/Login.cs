using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class Login
    {
        [DataMember]
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Email must be between 6 and 30 characters.")]
        [DisplayName("Email")]
        [RegularExpression(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$", ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Password is required.")]
        [DisplayName("Password")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        public string Password { get; set; }
    }
}
