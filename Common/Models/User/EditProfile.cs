using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class EditProfile
    {
        [DataMember]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 30 characters.")]
        [DisplayName("Username")]
        public string Username { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Email must be between 6 and 30 characters.")]
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [DataMember]
        [DisplayName("Confirm Old Password")]
        [Required(ErrorMessage = "Confirm Old Password is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        public string ConfirmOldPassword { get; set; }

        [DataMember]
        [Required(ErrorMessage = "New Password is required.")]
        [DisplayName("New Password")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        public string NewPassword { get; set; }

        [DataMember]
        [DisplayName("Confirm New Password")]
        [Required(ErrorMessage = "Confirm New Password is required.")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 30 characters.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }

        public EditProfile(User user)
        {
            Username = user.Username;
            Email = user.Email;
            ConfirmOldPassword = "";
            NewPassword = "";
            ConfirmNewPassword = "";
        }

        public EditProfile() { }
    }
}
