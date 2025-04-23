
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Common;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MailKit.Net.Smtp;
using MailKit.Security;
namespace AuthApi.Services
{
    public class EmailService
    {
        public string Sender { get; set; } = null;
        public string SmptServer { get; set; } = null;
        public int  Port { get; set; }
        public string UserName { get; set; } = null;
        public string Password { get; set; } = null;



        public EmailService(IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            Sender = emailSettings.GetValue<string>("UserEmail");
            SmptServer = emailSettings.GetValue<string>("SmptServer");
            Port = emailSettings.GetValue<int>("Port");
            UserName = emailSettings.GetValue<string>("UserName");
            Password = emailSettings.GetValue<string>("UserApiKey");
        }

        public bool CreateAndSendConfirmarionEmail(string email, string token) {
          return  SendEmail(CreateConfEmail(email, token));
        }
        public bool CreateAndSendPasswordRessetEmail(string email, string token)
        {
          return  SendEmail(CreateResetEmail(email, token));
        }

        public MimeMessage CreateConfEmail(string email, string token)
        {    
            MimeMessage mail = new();
            mail.From.Add(new MailboxAddress("email", Sender));
            mail.To.Add(new MailboxAddress("email", email));
            mail.Subject = "Confirmación de correo";

            string body = $"Hola,\n\nPor favor confirma tu correo usando el siguiente token:\n\n{token}\n\nEste token expirará en 1 hora.";

            mail.Body = new TextPart("plain")
            {
                Text = body
            };
            return mail;
        }

        public MimeMessage CreateResetEmail(string email, string token)
        {
            MimeMessage mail = new();
            mail.From.Add(new MailboxAddress("email", Sender));
            mail.To.Add(new MailboxAddress("email", email));
            mail.Subject = "Cambiar contraseña";

            string body = $"Hola,\n\nPor favor haz click en el enlace para cambiar tu contraseña usando el siguiente token:\n\n{token}\n\nEste token expirará en 1 hora.";

            mail.Body = new TextPart("plain")
            {
                Text = body
            };

            return mail;
        }

        public bool SendEmail(MimeMessage message)
        {
            SmtpClient client = new();
            try
            {
                client.Connect(SmptServer, Port, SecureSocketOptions.SslOnConnect);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(UserName, Password);
                client.Send(message);
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending email: {e.Message}");
                return false;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
               
            }

        }




    }
}
