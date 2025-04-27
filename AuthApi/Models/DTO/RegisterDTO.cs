namespace AuthApi.Models.DTO
{
    public class RegisterDTO
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
        public string Email { get; set; }
        public UserType UserType { get; set; }

    }
}
