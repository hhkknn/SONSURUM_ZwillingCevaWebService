using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ZwillingCevaWebService
{
    public class Logger
    {
        public static string LogFilePath { get; set; }

        public static void addLog(System.String Sender, System.String LogString)
        {
            /* <add key="LogFilePath" value="C:\AppLogs\LIS_Device_Connector_Urine"/> */
            //LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
            LogFilePath = @"C:\Siparis\Log";
            if (!System.IO.Directory.Exists(LogFilePath))
            {
                System.IO.Directory.CreateDirectory(LogFilePath);
            }

            if (!System.IO.File.Exists(System.String.Concat(string.Format("{0}\\", LogFilePath), System.DateTime.Now.ToString("yyyyMMdd"), "_", Sender.ToString().Trim(), ".txt")))
            {
                System.IO.File.Create(System.String.Concat(string.Format("{0}\\", LogFilePath), System.DateTime.Now.ToString("yyyyMMdd"), "_", Sender.ToString().Trim(), ".txt")).Close();
            }

            System.IO.StreamWriter writer = null;

            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(System.String.Concat(string.Format("{0}\\", LogFilePath), System.DateTime.Now.ToString("yyyyMMdd"), "_", Sender.ToString().Trim(), ".txt"), System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
                writer = new System.IO.StreamWriter(stream);
                writer.WriteLine(System.DateTime.Now.ToString("HH:mm:ss").Trim() + "|" + LogString.ToString().Trim());
                writer.Flush();
                writer.Close();
            }
            catch
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }
        }
    }
}