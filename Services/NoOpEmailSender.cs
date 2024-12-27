namespace UFCApp.Services
{
    using Microsoft.AspNetCore.Identity.UI.Services;
    using System.Threading.Tasks;

    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // No-op: Email sending is disabled
            return Task.CompletedTask;
        }
    }
}
