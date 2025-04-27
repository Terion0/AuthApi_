using AuthApi.Models;

namespace AutenticationApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool Confirmed { get; set; } = false;
        public UserType UserType { get; set; }  

    }
}
