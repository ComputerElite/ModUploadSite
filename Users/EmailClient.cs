using ComputerUtils.Logging;
using System.Net;
using System.Net.Mail;

namespace ModUploadSite.Users
{
    public class EmailClient
    {
        public string emailAddress { get; set; } = "";
        public string emailDisplayName { get; set; } = "";
        public string password { get; set; } = "";
        public string smtpServer { get; set; } = "";
        public int smtpPort { get; set; } = 587;
        public bool sslEnabled { get; set; } = true;

        public EmailClient() { }
        public EmailClient(string email, string password, string smtpServer, int port = 587, string emailDisplayName = "", bool sslEnabled = true)
        {
            this.emailAddress = email;
            this.password = password;
            this.smtpServer = smtpServer;
            this.smtpPort = port;
            this.emailDisplayName = emailDisplayName;
            this.sslEnabled = sslEnabled;
            Logger.Log("initialized Email client");
        }

        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                Logger.Log("Sending email to " + toEmail + ": " + subject);
                SmtpClient client = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = sslEnabled,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailAddress, password),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                MailMessage msg = new MailMessage(emailAddress, toEmail);
                msg.Subject = subject;
                msg.Body = body;
                client.Send(msg);
                Logger.Log("email sent");
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Exception while sending email:\n" + e.ToString(), LoggingType.Error);
                return false;
            }
        }
    }
}