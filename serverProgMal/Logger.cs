using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

using System.Collections.Concurrent;

using System.IO.Compression;

namespace serverProgMal
{
    class Logger
    {
        private string password;
        private string username;
        private Socket s;
        Thread t;
        DBConnect db;
        BlockingCollection<string> ac;

        public Logger(Socket sock, string username, string password, BlockingCollection<string> ac)
        {
            this.s = sock;
            this.password = password;
            this.username = username;
            //creo il thread passando la funzione come parametro
            t = new Thread(new ThreadStart(this.action));
            db = new DBConnect();
            this.ac = ac;
            t.Start();
        }

        public void GestoreClient()
        {
            byte[] bytes = new Byte[1024];
            try
            {   
                //ricevo la cartalle da sincronizzare
                int bytesRec = s.Receive(bytes);
                string cmd = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                //verifico il comando
                if (String.Compare(cmd.Substring(0, 2), "S:") != 0)
                {
                    byte[] msg = Encoding.ASCII.GetBytes("E.comando errato");
                    s.Send(msg);
                    s.Shutdown(SocketShutdown.Both);
                    throw new Exception("E.comando errato diverso da S:path");
                }

                //splitto la tringa
                char[] delimiterChars = {':'};
                string[] words = cmd.Split(delimiterChars);
                if (words.Length != 2)
                {
                    byte[] msg = Encoding.ASCII.GetBytes("E.comando errato");
                    s.Send(msg);
                    s.Shutdown(SocketShutdown.Both);
                    throw new Exception("E.comando errato words.Leght!=3");
                }

                //recupero il nome della directory che l'utente vuole sincronizzare
                //path to sync deve essere nella forma \cartella1\cartella2\ senza c:
                //cioè il Path assoluto senza la "c:"
                string pathIntoClient = words[1];
                
                //recupero la root dellutente nel server
                string userFolderIntoServer = System.IO.Path.Combine(MyGlobal.rootFolder, username);
                
                //cerco la cartella //DA CAMBIARE
                string pathIntoServer = System.IO.Path.Combine(userFolderIntoServer, pathIntoClient);
                if (Directory.Exists(pathIntoServer))
                {
                    sincronizzaFiles(pathIntoServer);
                }
                else
                {
                    sincronizzaDirectory(userFolderIntoServer, pathIntoClient, pathIntoServer);                    
                }
                Console.WriteLine("sincronizzazione Finita");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                s.Close();
                Console.WriteLine("Sessione terminata");
            }
            
        }

        public void sincronizzaDirectory(string ufis, string pic, string pis)
        {
            try
            {
                //creo la prima cartella di upload per il client
                //string pisu1 = System.IO.Path.Combine(MyGlobal.rootFolder, "u1");
                DirectoryInfo u1 = Directory.CreateDirectory(pis);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(pis));

                //invio OK
                byte[] msg = Encoding.ASCII.GetBytes("OK");
                s.Send(msg);
                
                //ricevo il file.zip
                //vediamo dove lo salva
                FileStream fStream = new FileStream("u1.zip", FileMode.Create);

                // read the file in chunks of 1KB
                var buffer = new byte[1024];
                int bytesRead;

                //leggo la lunghezza
                bytesRead = s.Receive(buffer);
                string cmdFileSize = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                int length = Convert.ToInt32(cmdFileSize);

                int received = 0;
                
                while (received < length)
                {
                    bytesRead = s.Receive(buffer);
                    received += bytesRead;
                    if(received >= length)
                    {
                        bytesRead = bytesRead - (received - length) ;
                    }
                    fStream.Write(buffer, 0, bytesRead);
                    
                    Console.WriteLine("ricevuti: " + bytesRead + " qualcosa XD");
                }
                fStream.Flush();
                fStream.Close();
                Console.WriteLine("File Ricevuto");
                 
                //estraggo
                ZipFile.ExtractToDirectory("u1.zip", pis+"\\u1");
                Console.WriteLine("file estratto");

                //invio un ultimo ok al client
                msg = Encoding.ASCII.GetBytes("OK");
                s.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                s.Close();
                Console.WriteLine("Sessione terminata");
            }
        }

        public void sincronizzaFiles(string path)
        {
            //OK.LIST
            //mando la lista dei file:
            //un file "percorso(radice la directory iniziale) hash" uno per riga
            //A
        }

        //public static string RandomString(int length)
        //{
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //    var random = new Random();
        //    return new string(Enumerable.Repeat(chars, length)
        //      .Select(s => s[random.Next(s.Length)]).ToArray());
        //}

        private void action()
        {
            string query = "select count(*) from utenti where username = " + "'" + username + "'";
            if (db.Count(query) > 0)//utente esistente
            {
                query = "select count(*) from utenti where username = " + "'" + username + "'" + "and password = " + "'" + password + "'";
                if (db.Count(query) > 0)//password corretta
                {
                    //invio OK
                    byte[] msg = Encoding.ASCII.GetBytes("OK");
                    s.Send(msg);
                    //Console.WriteLine("utente loggato con chiave: "+chiave);
                    Console.WriteLine("utente loggato ip: " + s.RemoteEndPoint);
                    GestoreClient();
                }
                else
                {
                    byte[] msg = Encoding.ASCII.GetBytes("E.password errata!");
                    s.Send(msg);
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                    Console.WriteLine("E.password errata!");
                }
            }
            else
            {
                byte[] msg = Encoding.ASCII.GetBytes("E.inesistente/password errata, registrati!");
                s.Send(msg);
                s.Shutdown(SocketShutdown.Both);
                s.Close();
                Console.WriteLine("E.inesistente, registrati!");
            }
        }
    }    
}
