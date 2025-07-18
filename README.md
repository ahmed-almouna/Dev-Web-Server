# Web-Server
A simple HTTP server implemented in C# that handles HTTP GET requests over TCP/IP. It serves files such as HTML, JPG, TXT, and GIF, making it ideal for local development and testing environments. 

## Features:
- Handles HTTP GET requests.
- Serves static files: HTML, JPG, TXT, and GIF.
- Built using C# and TCP/IP connections.
- Supports custom directory, IP, and port via command-line arguments.
  
## Getting Started
* Running the program using Visual Studio is not required but is recommended.

### Installation
* Download and extract the zip folder, or clone the repository `git clone https://github.com/ahmed-almouna/Web-Server.git`.
* Open the installed folder.
* Run the program by executing *Web-Server.exe* in command-line or running the solution in Visual Studio.

## Usage
The server needs the following arguments:
   - `-DIRECTORY`: Specify the directory containing the files.
   - `-IP_ADDRESS`: Specify the IP address the server should listen to.
   - `-PORT`: Specify the port the server should listen to.

For example:
   ```
   Web-Server.exe C:\localWebSite 172.2.16.10 14000
   ```
The server will now listen for requests on 172.2.16.10 14000, and serve files found in C:\localWebSite.

  Once the server is running, open up a browser, navigate to `http://172.2.16.10:14000/`, and start requesting files!

