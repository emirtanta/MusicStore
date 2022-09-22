using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public EmailSender(IOptions<EmailOptions> options) 
        {
            _emailOptions = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Exceute(_emailOptions.SendGridApiKey,subject,htmlMessage,email);
        }

        private  Task Exceute(string sendGridApiKey,string subject,string message,string email)
        {
            //var apiKey = Environment.GetEnvironmentVariable("NAME_OF_THE_ENVİRONMETN_VARAIBLE_FOR_YOUR_SENGRID_KEY");
            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress("emirtanta41@gmail.com", "Music Store");
            var to = new EmailAddress(email, "end user");
            var msg = MailHelper.CreateSingleEmail(from, to, subject,"",message);

            return  client.SendEmailAsync(msg);
        }
    }
}
