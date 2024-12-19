using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*
*   File          : Program.cs
*   Project       : PROG2001 - A5
*   Programmer    : Ahmed Almoune
*   First Version : 11/24/2024
*   Description   :
*      The class in this file is used to create a web server. It takes CLI arguments to determine the specifics of the server (i.e. root 
*      directory, IP, Port, etc.), and then it starts up a server to listen for requests and respond to those requests accordingly. 
*/
namespace myOwnWebServer
{
    internal class Program
    {
        static int Main(string[] args)
        {
            /* to define valid arguments' formats */
            Regex validRoot = new Regex(@"^(.*\S.*)$");
            Regex validIP = new Regex(@"^(\d{1,3}[.]\d{1,3}[.]\d{1,3}[.]\d{1,3})$");
            Regex validPort = new Regex(@"^(\d+)$");

            /* if any of the arguments are in an invalid format; show usage message */
            if ((args.Length != 3) || (validRoot.IsMatch(args[0]) == false) || (validIP.IsMatch(args[1]) == false) ||
                (validPort.IsMatch(args[2]) == false))
            {
                Console.WriteLine("Usage: myOwnWebServer <Directory> <IP address> <Port number> \n <Directory>  : the folder containing " +
                    "the assets (e.g. C:\\localWebSite) \n <IP address> : the IP address the server will listen to (e.g. 192.168.100.23)" +
                    "\n <Port number>: the port number the server will listen to (e.g 5300) \n");

                return 1;
            }

            /* parse info from arguments */
            string rootDirectory = args[0];
            IPAddress ipAddress = null;
            bool workingIP = IPAddress.TryParse(args[1], out ipAddress);
            int port = int.Parse(args[2]);

            /* additional input validation; format is correct but something is wrong */
            if (!Directory.Exists(rootDirectory))
            {
                Console.WriteLine("Specified directory does not exist. choose another one");
                return 2;
            }
            if (!workingIP)
            {
                Console.WriteLine("IP address is invalid. choose a valid IP address");
                return 3;
            }
            if (port > 65535) //number of ports possible
            {
                Console.WriteLine("Port number is too large. must be below 65,536.");
                return 4;
            }

            IPEndPoint socketEndPoint = new IPEndPoint(ipAddress, port);

            /* create & start a server using that info */
            Server server = new Server(rootDirectory, socketEndPoint);
            server.StartListener();

            return 0;
        }

    }
}
