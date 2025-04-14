namespace AuthApi.Models.DTO
{
    public class ChangePasswordDTO
    {
        public string Token { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}
