using System;
using System.Configuration;
using System.Net.Mail;

namespace SvnManager.WebUI
{
    class Notification
    {
        static public void SendError(Exception ex)
        {
            SendError(ex, "");
        }
        static public void SendError(Exception ex, string note)
        {
            // that send process does not include default bcc copying
            using (MailMessage m = new MailMessage(ConfigurationManager.AppSettings["Manger.SendEmailFrom"], ConfigurationManager.AppSettings["Manger.SendErrorsTo"]))
            {
                m.Subject = "SVN Manager Error" + (note.Length > 0 ? " (" + note + ")" : "");
                m.Body = GetFullError(ex);

                using (SmtpClient c = new SmtpClient())
                {
                    try
                    {
                        c.Send(m);
                    }
                    catch (Exception)
                    { }
                }
            }
        }
        static private string GetFullError(Exception ex)
        {
            string header = "";
            if (ex != null)
            {
                Exception headerEx = ex;
                if (headerEx.InnerException != null)
                    headerEx = headerEx.InnerException;
                if (headerEx != null)
                    header = headerEx.Message + "\r\n\r\n";
            }

            return
                header +
                "Error Type:  SVN Manager Application Error\n" +
                "Exception: \n" + ExcDetails.Get(ex);
        }
    }
}