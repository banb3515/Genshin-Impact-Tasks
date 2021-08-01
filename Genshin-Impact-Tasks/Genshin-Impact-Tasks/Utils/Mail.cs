using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Genshin_Impact_Tasks.Utils
{
    public class Mail
    {
        private readonly string SenderID = "YOUR GOOGLE ID";
        private readonly string SenderPassword = "YOUR GOOGLE PASSWORD";
        private readonly string DisplayName = "원신 태스크 (Genshin Impact Tasks)";

        private readonly MailMessage MailMsg;

        public Mail(string subject, string body, string toMail)
        {
            try
            {
                MailMsg = new MailMessage();
                MailMsg.From = new MailAddress($"{SenderID}@gmail.com", DisplayName, Encoding.UTF8);
                MailMsg.To.Add(toMail);
                MailMsg.Subject = subject;
                MailMsg.Body = body;
                MailMsg.IsBodyHtml = true;
                MailMsg.SubjectEncoding = Encoding.UTF8;
                MailMsg.BodyEncoding = Encoding.UTF8;
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }

        public async void Send()
        {
            try
            {   
                var smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(SenderID, SenderPassword);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                await smtp.SendMailAsync(MailMsg);
            }
            catch (Exception ex)
            {
                App.DisplayEx(ex);
            }
        }
    }
}
