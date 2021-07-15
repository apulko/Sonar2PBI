using System;
using System.Net.Mail;
using System.Text;

namespace Sonar2PBI
{
    public static class SmtpHelper
    {
        public static void EpostaGonder(string body, string subject)
        {
            EpostaGonder(body, subject, string.Empty);
        }
        public static void EpostaGonder(string body, string subject, string emailAdress)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigHelper.SmtpServer))
                    return;


#if DEBUG
                emailAdress = ConfigHelper.ToAdress;
#endif


                //TO Adresi boş gelmiş ise Config'tten okumaya çalışır.
                //Configte bir TO adrsi yok ise epost gönderme metodunu terk eder.
                if (emailAdress.IsNullOrWhiteSpace())
                {
                    emailAdress = ConfigHelper.ToAdress;

                }
                emailAdress = SanitizeEmail(emailAdress);
                if (emailAdress.IsNullOrWhiteSpace())
                    return;

              
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(ConfigHelper.FromAdress),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = body,
                    BodyEncoding = Encoding.Unicode,
                };
                mail.To.Add(emailAdress);
                mail.Bcc.Add(ConfigHelper.ToAdress);
                mail.AlternateViews.Add(System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, new System.Net.Mime.ContentType("text/html")));

                using (var smtpClinet = new SmtpClient(ConfigHelper.SmtpServer))
                {
                    smtpClinet.Send(mail);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Logla(ex);
            }
        }
        private static string SanitizeEmail(string email)
        {
            return email.Replace(";", ",").Trim();
        }
    }
}