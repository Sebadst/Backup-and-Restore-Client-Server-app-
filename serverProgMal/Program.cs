using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using System.Collections.Concurrent;

namespace serverProgMal
{
    class Program
    {
        // Incoming data from the client.
        public static string data = null;
        

        //funzione
        public static void recognizeCommand(string str, Socket sock, BlockingCollection<string> ac)
        {
            //Console.WriteLine("dentro");
            if(String.Compare(str.Substring(0,2), "R.") != 0 && String.Compare(str.Substring(0, 2), "L.") != 0)
            {
                Console.WriteLine("E.comando errato");
                byte[] msg = Encoding.ASCII.GetBytes("E.comando errato");
                sock.Send(msg);
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
                return;
            }
            
            char[] delimiterChars = { '.', '\t', '<' };
            string[] words = str.Split(delimiterChars);

            if(words.Length != 4)
            {
                Console.WriteLine("E.comando errato");
                byte[] msg = Encoding.ASCII.GetBytes("E.comando errato");
                sock.Send(msg);
                sock.Shutdown(SocketShutdown.Both);
                sock.Close();
                return;
            }
            string username = words[1];
            string password = words[2];
            //string pathToSync = words[3];

            if (str.ToCharArray()[0] == 'R')
            {
                Register r = new Register(sock, username, password);
            }

            if (str.ToCharArray()[0] == 'L')
            {
                Logger l = new Logger(sock, username, password, ac);
            }
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            //processi attivi
            //List<string> connessioniAttive = new List<string>();
            BlockingCollection<string> connessioniAttive = new BlockingCollection<string>();

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("serverAddress: "+localEndPoint.ToString());
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    data = null;

                    Console.WriteLine("new connection from: " + handler.RemoteEndPoint);

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", data);
                    recognizeCommand(data, handler, connessioniAttive);
                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }


        static int Main(string[] args)
        {
            StartListening();
            return 0;
        }
    }
}
