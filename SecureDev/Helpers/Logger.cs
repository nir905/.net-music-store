using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Vladi2.Models;

namespace Vladi2.Helpers
{
    public class Logger
    {
        public const string SQLLiteMsg = "SQLiteException";
        public const string LoginSuccess = "Login Success";

        private static readonly string FilePath = HttpContext.Current.Server.MapPath("~/App_Data/Error.log");

        public static void WriteToLog(string msg)
        {
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine("----------")
                .AppendLine(String.Format("{0} by {1} at {2}",DateTime.Now.ToString(),ExceptionOwner(), HttpContext.Current.Request.Url.AbsolutePath))
                .AppendFormat(msg)
                .AppendLine();

            using (StreamWriter writer = File.AppendText(FilePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }
        public static void WriteToLog(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine("----------")
                .AppendLine(String.Format("{0} by {1} at {2}", DateTime.Now.ToString(), ExceptionOwner(), HttpContext.Current.Request.Url.AbsolutePath))
                .AppendFormat("Source:\t{0}", exception.Source)
                .AppendLine()
                .AppendFormat("Target:\t{0}", exception.TargetSite)
                .AppendLine()
                .AppendFormat("Type:\t{0}", exception.GetType().Name)
                .AppendLine()
                .AppendFormat("Message:\t{0}", exception.Message)
                .AppendLine()
                .AppendFormat("Stack:\t{0}", exception.StackTrace)
                .AppendLine();

            using (StreamWriter writer = File.AppendText(FilePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }

        private static string ExceptionOwner()
        {
            return HttpContext.Current.Session["myUser"] == null ? "Anonymous" : (HttpContext.Current.Session["myUser"] as User).UserName;
        }
    }
}