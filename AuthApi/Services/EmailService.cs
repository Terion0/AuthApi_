
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Common;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MailKit.Net.Smtp;
using MailKit.Security;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Reflection.Metadata;
namespace AuthApi.Services
{
    public class EmailService
    {
        public string Sender { get; set; } = null;
        public string UserName { get; set; } = null;
        public string Password { get; set; } = null;
        public string Host { get; set; } = null;


        public EmailService(IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            Sender = emailSettings.GetValue<string>("UserEmail");
            UserName = emailSettings.GetValue<string>("UserName");
            Password = emailSettings.GetValue<string>("UserApiKey");
            Host = emailSettings.GetValue<string>("Host");
        }

        public bool CreateAndSendConfirmarionEmail(string email, string token) {
          return  SendConEmail(email, token).Result;
        }
        public bool CreateAndSendPasswordRessetEmail(string email, string token)
        {
          return  SendResetEmail(email, token).Result;
        }

      


        public async Task<bool> SendConEmail(string toEmail, string token)
        {
            string confirmationLink = $"http://{Host}/confirm-email/{token}";
            var client = new SendGridClient(Password);
            var from = new EmailAddress(Sender, UserName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, "Confirmation email", confirmationLink, htmlContent: null);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> SendResetEmail(string toEmail, string token)
        {
            string confirmationLink = $"http://{Host}/pass-change/{token}";
            var client = new SendGridClient(Password);
            var from = new EmailAddress(Sender, UserName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, "Change password", confirmationLink, htmlContent: null);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }


    }
}
