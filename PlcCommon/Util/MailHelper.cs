using PlcCommon.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlcCommon.Util
{
    public class MailHelper
    {
        const string From = "automail@ors.com.tr";
        const string Password = "ors2020";
        const int Port = 587; //POP3 port 110/SMTP
        const string Host = "mail.ors.com.tr";

        public static string[] Adresler = new string[] { "mustafa.bayer@uyumsoft.com", "huseyin.celik@uyumsoft.com" };
        public static string MailBaslik = "Otomasyon Bilgi (OPC Client)";

        //const string From = "huseyin.celik@yandex.com";
        //const string Password = "";//"ors123";
        //const int Port = 587; //POP3 port 110/SMTP port 587
        //const string Host = "smtp.yandex.ru";

        public static void SendMail(string[] address, string subject, string body, string fileAttachment = null)
        {
            Task.Run(() =>
            {
                Send(address, null, subject, body, fileAttachment);
            });
        }

        public static void Send(string[] address, string[] cc, string subject, string body, string fileAttachment = null)
        {
            try
            {

                MailMessage mesaj = new MailMessage();
                mesaj.From = new MailAddress($"Otomasyon bilgi <{From}>");
                if (address != null && address.Length > 0)
                {
                    foreach (string item in address)
                    {
                        mesaj.To.Add(new MailAddress(item, item));
                    }
                }
                else
                {
                    mesaj.To.Add(new MailAddress("huseyin.celik@uyumsoft.com", "Hüseyin ÇELİK <huseyin.celik@uyumsoft.com>"));
                    //mesaj.To.Add(new MailAddress("huseyin.clk@hotmail.com", "Hüseyin ÇELİK <huseyin.clk@hotmail.com>"));
                    ////mesaj.To.Add(new MailAddress("mustafa.bayer@uyumsoft.com", "Mustafa BAYER <mustafa.bayer@uyumsoft.com>"));
                    //mesaj.To.Add(new MailAddress("tolga.ozdag@uyumsoft.com", "Tolga ÖZDAĞ <tolga.ozdag@uyumsoft.com>"));
                    ////mesaj.To.Add(new MailAddress("huseyin.kilinc@ors.com.tr", "Hüseyin KILINÇ <huseyin.kilinc@ors.com.tr>"));
                }
                if (cc != null && cc.Length > 0)
                {

                    foreach (string item in cc)
                    {
                        mesaj.CC.Add(new MailAddress(item, item));
                    }
                }

                mesaj.Subject = subject;
                mesaj.IsBodyHtml = false;
                mesaj.Body = body;
                if (fileAttachment != null)
                {
                    // Create attachment by using existing fileStream.
                    Attachment data = new Attachment(new FileStream(fileAttachment, FileMode.OpenOrCreate), System.Net.Mime.MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                    disposition.FileName = new FileInfo(fileAttachment).Name;
                    disposition.Size = data.ContentStream.Length;
                    disposition.CreationDate = System.IO.File.GetCreationTime(fileAttachment);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(fileAttachment);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(fileAttachment);
                    // Add the attachment to this message.
                    mesaj.Attachments.Add(data);
                }
                SmtpClient smtp = new SmtpClient(Host) { Port = Port };
                System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(From, Password);
                smtp.UseDefaultCredentials = true;
                smtp.EnableSsl = false;
                smtp.Timeout = 100000 * 3; // default 100000   imiş 3 ile çarptım.
                smtp.Credentials = SMTPUserInfo;
                smtp.Send(mesaj);
                //smtp.SendMailAsync(mesaj);
                //Task.Run(() =>
                //{
                //    smtp.Send(mesaj);
                //});

                //if (Bcc.Count > 0)
                //{
                //    foreach (Contact item in Bcc)
                //    {
                //        message.Bcc.Add(new MailAddress(item.Email, item.FullName));
                //    }
                //}
            }
            catch (SmtpException E)
            {
                Logger.E("Mail send failed with message: " + E.Message);
                //return "Mail send failed with message: " + E.Message;

            }

            //return "Mail was send successfully";

        }

        public static bool IsValidEmailAddress(string emailaddress)
        {
            string pattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
  + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
  + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
  + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
  + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
  + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
            return !string.IsNullOrWhiteSpace(emailaddress) && Regex.IsMatch(emailaddress, pattern);

            //return Regex.IsMatch(emailaddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
