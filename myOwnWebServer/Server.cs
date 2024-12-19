using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*
*   File          : Server.cs
*   Project       : PROG2001 - A5
*   Programmer    : Ahmed Almoune
*   First Version : 11/24/2024
*   Description   :
*      The class in this file represents a web server. It can listen for requests over a specified socket and respond appropriately using 
*      the HTTP protocol.
*/
namespace myOwnWebServer
{
    internal class Server
    {
        /* constants */
        const int kRequestMessageSize = 16383; //the maximum expected size of a request message (in bytes)

        /* data members */
        private string root = string.Empty;
        private IPEndPoint socket = null;

        private Log log = new Log(); //logger class

        /*
        *  Method  : Server()
        *  Summary : creates an instance of the Server class with the specified parameters. 
        *  Params  : 
        *     string rootDirectory      = the directory which contains the files the server can access (.txt, .jpg, .gif, etc.)
        *     IPEndPoint socketEndPoint = the socket which the server will be listening to.
        *  Return  :  
        *     none.
        */
        internal Server(string rootDirectory, IPEndPoint socketEndPoint)
        {
            root = rootDirectory;
            socket = socketEndPoint;
        }

        /*
        *  Method  : StartListener()
        *  Summary : starts listening for client requests, and responds accordingly. 
        *  Params  : 
        *     none.
        *  Return  :  
        *     none.
        */
        internal void StartListener()
        {
            try
            {
                TcpListener server = new TcpListener(socket);
                server.Start();
                log.Write($"[SERVER STARTED] - webRoot={root} webIP={socket.Address.ToString()} webPort={socket.Port.ToString()}");

                Console.WriteLine("Working... ");
                /* enter a listening loop */
                while (true)
                {
                    /* wait for and accept a connection */
                    TcpClient client = server.AcceptTcpClient();
                    //Console.WriteLine("A connection occurred!");
                    NetworkStream stream = client.GetStream();

                    /* read request */
                    byte[] requestBuffer = new byte[kRequestMessageSize];
                    stream.Read(requestBuffer, 0, requestBuffer.Length);
                    string requestMessage = Encoding.ASCII.GetString(requestBuffer);
                    //Console.WriteLine("Received: \n" + requestMessage);

                    /* handle request */
                    string responseMessage = HandleRequest(requestMessage);

                    /* send response */
                    byte[] responseBuffer = Encoding.ASCII.GetBytes(responseMessage);
                    stream.Write(responseBuffer, 0, responseBuffer.Length);
                    //Console.WriteLine("Sent: \n" + responseMessage);

                    client.Close();
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("A socket error occurred.. please ensure IP & port are valid.");
                log.Write("[ERROR] - A socket error occurred.. please ensure IP & port are valid.");

            }
            catch (IOException ioe)
            {
                Console.WriteLine("An I/O error occurred.. please ensure request being sent is valid.");
                log.Write("[ERROR] - An I/O error occurred.. please ensure request being sent is valid.");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred..");
                log.Write("[ERROR] - An error occurred..");
            }
        }

        /*
        *  Method  : HandleRequest()
        *  Summary : takes a string argument repressing an HTTP request from a client and provides an appropriate response. 
        *  Params  : 
        *     string requestMessage = the client request.
        *  Return  :  
        *     string responseMessage = the response to the client.
        */
        internal string HandleRequest(string requestMessage)
        {
            /* response' elements */
            string responseCode = string.Empty;
            DateTime timeStamp = DateTime.Now;
            string server = "myOwnWebServer";
            string responseContentType = string.Empty;
            string responseBody = string.Empty;
            string responseMessage = string.Empty;

            /* define error codes */
            Regex goodRequest = new Regex(@"^(.*) (/.*) (HTTP/1.1)(\r\n)(?i:Host:.+)(\r\n)(.|\n)*(\r\n)(.*)"); 
            Regex supportedMediaType = new Regex(@"^(.*[.](txt|html|htm|jpeg|jpg|gif))$");

            string[] fileTypes = { ".txt", ".html", ".htm", ".jpeg", ".jpg", ".gif" };
            string[] mimeTypes = { "text/plain", "text/html", "text/html", "image/jpeg", "image/jpeg", "image/gif" };

            //400
            if (goodRequest.IsMatch(requestMessage) == false)
            {
                responseCode = "400 Bad Request";
                responseContentType = mimeTypes[0];
                responseBody = "Request is invalid, please try again.";
                responseMessage = $"HTTP/1.1 {responseCode}\r\nDate: {timeStamp}\r\nServer: {server}\r\nContent-Type: " +
                    $"{responseContentType}\r\nContent-Length: {responseBody.Length.ToString()}\r\n\r\n{responseBody}";
                LogBadRequest(requestMessage);
                log.Write($"[RESPONSE] - Status Code={responseCode}");
                return responseMessage;
            }

            string[] requestParts = requestMessage.Split(' ');
            string verb = requestParts[0];
            string filePath = requestParts[1];
            log.Write($"[REQUEST]  - verb={verb} resource={filePath}");

            //405
            if (verb != "GET")
            {
                responseCode = "405 Method Not Allowed";
                responseContentType = mimeTypes[0];
                responseBody = "Method used is not allowed, please use GET.";
                responseMessage = $"HTTP/1.1 {responseCode}\r\nDate: {timeStamp}\r\nServer: {server}\r\nContent-Type: " +
                    $"{responseContentType}\r\nContent-Length: {responseBody.Length.ToString()}\r\n\r\n{responseBody}";
                log.Write($"[RESPONSE] - Status Code={responseCode}");
                return responseMessage;
            }

            //415
            if (supportedMediaType.IsMatch(filePath) == false)
            {
                responseCode = "415 Unsupported Media Type";
                responseContentType = mimeTypes[0];
                responseBody = "Requested file type is not supported.";
                responseMessage = $"HTTP/1.1 {responseCode}\r\nDate: {timeStamp}\r\nServer: {server}\r\nContent-Type: " +
                    $"{responseContentType}\r\nContent-Length: {responseBody.Length.ToString()}\r\n\r\n{responseBody}";
                log.Write($"[RESPONSE] - Status Code={responseCode}");
                return responseMessage;
            }

            //404
            string fullPath = root + filePath;
            if (!File.Exists(fullPath))
            {
                responseCode = "404 Not Found";
                responseContentType = "text/plain";
                responseBody = "Requested file was not found.";
                responseMessage = $"HTTP/1.1 {responseCode}\r\nDate: {timeStamp}\r\nServer: {server}\r\nContent-Type: " +
                    $"{responseContentType}\r\nContent-Length: {responseBody.Length.ToString()}\r\n\r\n{responseBody}";
                log.Write($"[RESPONSE] - Status Code={responseCode}");
                return responseMessage;
            }
            
            /* open requested file and parse its content */
            FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            byte[] fileContentsBuffer = new byte[fs.Length];
            BinaryReader r = new BinaryReader(fs);
            r.Read(fileContentsBuffer, 0, fileContentsBuffer.Length);
            fs.Close();

            /* determine mime type */
            string fileType = Path.GetExtension(fullPath);
            string mimeType = string.Empty;
            for (int i = 0; i < fileTypes.Length; i++)
            {
                if (fileType == fileTypes[i])
                {
                    mimeType = mimeTypes[i];
                    break;
                }
            }

            //200
            responseCode = "200 Ok";
            responseContentType = mimeType;
            responseBody = Encoding.ASCII.GetString(fileContentsBuffer);
            responseMessage = $"HTTP/1.1 {responseCode}\r\nDate: {timeStamp}\r\nServer: {server}\r\nContent-Type: " +
                $"{responseContentType}\r\nContent-Length: {responseBody.Length.ToString()}\r\n\r\n{responseBody}";

            log.Write($"[RESPONSE] - Content-Type={responseContentType} Content-Length={responseBody.Length.ToString()} Server={server}");
            return responseMessage;
        }

        /* considering that a bad request (400) doen't necessarily follow HTTP format; this method attempts to try and parse 
         * what might be helpful info for the log */
        void LogBadRequest(string requestMessage)
        {
            string[] requestParts = requestMessage.Split(' ');
            if (requestParts.Length > 1)
            {
                if (requestParts[1].Contains("\n"))
                {
                    requestParts[1] = requestParts[1].Replace("\r\n", "");
                }

                log.Write($"[REQUEST]  - verb={requestParts[0]} resource={requestParts[1]}");
            }
            else
            {
                log.Write("[REQUEST]  - Unable to parse verb and resource");
            }
        }

        
    }
}
