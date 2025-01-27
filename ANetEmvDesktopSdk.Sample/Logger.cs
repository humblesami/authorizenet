using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANetEmvDesktopSdk.Sample
{
    public class Logger
    {
        string log_file_path = "aa_log.txt";
        public Logger() { 
            
        }
        public Logger(string file_path)
        {
            this.log_file_path = file_path;
        }
        public void log(string message, Exception ex = null, bool clear_all=false)
        {
            if (!File.Exists(this.log_file_path))
            {
                using (FileStream fs = File.Create(this.log_file_path)) { }
            }
            if (ex != null)
            {
                message += message + " => " + ex.ToString();
            }
            message = DateTime.Now.ToString("T") + " " + message;
            using (var sw = new StreamWriter(log_file_path, clear_all))
            {
                sw.WriteLine(message);
            }
        }
    }
}
