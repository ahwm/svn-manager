using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
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
            MailMessage m = new MailMessage("", "");
            m.Subject = "SVN Manager Error" + (note.Length > 0 ? " (" + note + ")" : "");
            m.Body = GetFullError(ex);

            try
            {
                SmtpClient c = new SmtpClient();
                c.Host = "";
                c.Port = 587;
                c.EnableSsl = true;
                c.Credentials = new NetworkCredential("", "");
                c.Send(m);
            }
            catch (Exception)
            { }
            m.Dispose();
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
                "Error Type:  SVN Application Error\n" +
                "Exception: \n" + ExcDetails.Get(ex);
        }
    }
}