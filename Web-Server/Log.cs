using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
*   File          : Log.cs
*   Project       : Web-Server
*   Programmer    : Ahmed Almoune
*   First Version : 11/24/2024
*   Description   :
*      The class in this file represents a logger class. it allows other classes to log their activities into a time-stamped log file.
*/
namespace WebServer
{
    internal class Log
    {
        private readonly string fullPath = "Web-Server.Log";
        private StreamWriter sw;

        /* create/overwrite the file */
        internal Log()
        {
            sw = File.CreateText(fullPath);
            sw.Close();
        }

        /* write an entry to the log */
        internal void Write(string newEntry)
        {
            sw = File.AppendText(fullPath);
            DateTime now = DateTime.Now;
            sw.WriteLine($"{now.ToString("yyyy-MM-dd HH:mm:ss")} {newEntry}");
            sw.Close();
        }


    }
}
