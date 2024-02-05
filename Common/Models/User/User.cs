using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        public User() { }

        public User(UsersTable user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
        }

        public User(Register user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.Password;
        }

        public User(EditProfile user)
        {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Password = user.NewPassword;
        }
    }
}
