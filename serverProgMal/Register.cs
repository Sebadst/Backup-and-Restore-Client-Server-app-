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

namespace serverProgMal
{
    class Register
    {
        private string password;
        private string username;
        private string pathToSync;
        private Socket s;
        Thread t;
        DBConnect db;
        
        public Register(Socket s, string username, string password)
        {
            this.s = s;
            this.password = password;
            this.username = username;
            //this.pathToSync = pathToSync;
            //creo il thread passando la funzione come parametro
            t = new Thread(new ThreadStart(this.action));
            db = new DBConnect();
            t.Start();
        }

        private void action()
        {
            string query = "select count(*) from utenti where username = " + "'" + username + "'";
            if(db.Count(query) <= 0)
            {

                query = "insert into utenti(uid, username, password) values(" + 0 + "," + "'" + username + "'" + "," + "'" + password + "'" + ")";
                db.Insert(query);
                byte[] msg = Encoding.ASCII.GetBytes("OK");
                s.Send(msg);
                s.Shutdown(SocketShutdown.Both);
                s.Close();
                Console.WriteLine("utente inserito");
                //create folder for user
                string pathString = System.IO.Path.Combine(MyGlobal.rootFolder, username);
                try
                {
                    // Determine whether the directory exists.
                    if (Directory.Exists(pathString))
                    {
                        Console.WriteLine("That path exists already. CONTROLLARE IL CODICE QUI CLASSE REGISTER");
                        //return;
                    }
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(pathString);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(pathString));
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }
            }
            else
            {
                byte[] msg = Encoding.ASCII.GetBytes("E.utenteEsistente/erroreInserimento");
                s.Send(msg);
                s.Shutdown(SocketShutdown.Both);
                s.Close();
                Console.WriteLine("E.utenteEsistente/erroreInserimento");
            }
        }
    }
}
