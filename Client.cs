using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;
namespace ProgettoPDS
{
    public class Client
        //R: signup
        //L: login
        //S: sendzip,synchronize
        //U: synch
        //A: ask presence of folder
    {
        private TcpClient tcpclnt;
        private string ipAddress;
        private int port;
        private string username;
        private string password;
        private Dictionary<string, string> file_hash = new Dictionary<string, string>();
        bool update_viewfolder = false;
      //  private string cookie;
        public Client(string username, string password){
            this.ipAddress = "192.168.1.21";
            this.port = 11000;
           //tcpclnt = new TcpClient();
            this.username = username;
            this.password = password;
        }
       
        public TcpClient Tcpclnt
        {
            get;
            set;
        }
       
        //todo aggiustare tutte le receive
        public bool receive(){
            try
            {
                byte[] rcv = new byte[1500];
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    if (String.Compare(rx.Substring(0, 2), "OK") == 0)
                        return true;
                    else return false;
                }
                else return false;
            }
            catch (Exception ex)
            {
                throw;
            }
            }
        public  void connect_to_server()
        {
            try
            {
                tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");

                var result=tcpclnt.BeginConnect(ipAddress, port,null,null);

                
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    Console.WriteLine("Failed to connect");
                }
                // use the ipaddress as in the server program
                else
                {
                    Console.WriteLine("Connected");
                    /*
                    byte[] msg = Encoding.UTF8.GetBytes("ciao giorgio");
                    byte[] rcv =new byte[1500];
                    tcpclnt.Client.Send(msg,SocketFlags.None);
                    int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                    if (byteCount > 0)
                        Console.WriteLine(Encoding.UTF8.GetString(rcv));
                     * */
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public int signup(string username,string password)
        {
            
            try
            {
                byte[] credentials = Encoding.UTF8.GetBytes("R"+"."+username+"."+password+"<EOF>");
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx =(string) Encoding.UTF8.GetString(rcv).Clone();
                    //Console.WriteLine(Encoding.UTF8.GetString(rcv));
                   // Console.WriteLine(rx);
                    if (String.Compare(rx,"OK")==0)
                    {
                        //procedo al signup
                        this.username = username;
                        this.password = password;
                        return 1;
                    }
                    else
                    {

                        
                        //chiedo se vuole riprovare il signup o meno
                        return 0;
                    }
                }
                return -1;   
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return -1;
            }
            finally
            {
                tcpclnt.Client.Close();
            }
        }
        
        public int login(string username,string password)
        {
            
            try
            {
                byte[] credentials = Encoding.UTF8.GetBytes("L" + "." + username + "." + password + "<EOF>");
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                   
                  //  if (String.Compare(rx.Substring(0,2), "OK") == 0)
                    if(String.Compare(rx,"OK")==0)
                    {
                        
                        return 1;
                    }
                    else
                    {


                        //chiedo se vuole riprovare il signup o meno
                        return 0;
                    }
                }
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return -1;
            }

        }

        public string ask_presence(string username)
        {
            try
            {
                byte[] credentials = Encoding.UTF8.GetBytes("A" + ":" + username);
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    rx=rx.Substring(0,rx.IndexOf('\0'));
                    //  if (String.Compare(rx.Substring(0,2), "OK") == 0)
                    return rx;
                }
                else
                {
                    return "Error connection with the server";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
                return "Exception in ask presence";
            }
        }
        public int send_zip(string path2,string file)
        {
            try
            {
                int l = path2.Length - 3;
                string path = path2.Substring(3, l);
                //invio s
                //lui controlla se esiste gia il percorso. se esiste
                byte[] credentials = Encoding.UTF8.GetBytes("S" + ":" + path);
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                //ricevo ok
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    //invio file
                    if (String.Compare(rx, "OK") == 0)
                    {

                        wrap_send_file(file);

                    }
                }

                //ricevo ok
                int byteCount2 = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount2 > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    if (String.Compare(rx, "OK") != 0)
                    {
                        Console.WriteLine("qualcosa e' andato storto");
                    }
                }

                //chiudo socket
                tcpclnt.Client.Close();
                return 1;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void browse_folder_json(string filename)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(filename);
                foreach (var dir in d.GetDirectories())
                    browse_folder_json(dir.FullName);
                foreach (var file in d.GetFiles())
                {
                    Console.WriteLine(file.FullName);
                    string hash = compute_md5(file.FullName);
       
                    //LO SCRIVO SU UN DICTIONARY
                    //faccio dictionary[namefile]=hash e poi lo posso serializzare su synch
                   /*
                    int location = file.FullName.IndexOf(username);
                    int l = file.FullName.Length - location - 2;
                    string namefile = file.FullName.Substring(location + 2, l);
                    * */
                    int l = file.FullName.Length - 3;
                    string namefile = file.FullName.Substring(3, l);
                    file_hash.Add(namefile, hash);
                }
            }
            catch
            {
                Console.WriteLine("Eccezione in browse folder json");
            }
        }
        //TODO metti in tmp
        public void browse_folder(string filename)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(filename);
                foreach (var dir in d.GetDirectories())
                    browse_folder(dir.FullName);
                foreach (var file in d.GetFiles())
                {
                    Console.WriteLine(file.FullName);
                    string hash = compute_md5(file.FullName);

                    string path = @"C:\Users\sds\Desktop\" + username + ".txt";
                    //string path = @"C:\Users\sds\Desktop\file.txt";
                    //se il file non esiste ne crea uno nuovo altrimenti lo usa
                    //con true fa l'append, con false la write
                    //quindi mi sa che devo mettere false
                    //invece e true
                    StreamWriter sw = new StreamWriter(path, true);
                    //NUOVO
                    int l = file.FullName.Length - 3;
                    string namefile = file.FullName.Substring(3, l);
                    sw.WriteLine(namefile + " " + hash);
                    //OPPURE IN JSON
                    //LO SCRIVO SU UN DICTIONARY
                    //faccio dictionary[namefile]=hash e poi lo posso serializzare su synch
                    sw.Close();
                }
            }
            catch
            {
                Console.WriteLine("Eccezione in browse folder");
            }

        }

        public string compute_md5(string filename)
        {
            var md5 = MD5.Create();

            var stream = File.OpenRead(filename);

            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();


        }
        // USARE QUESTA
        public void wrap_send_file(string file)
        {
            try
            {
                StreamWriter sWriter = new StreamWriter(tcpclnt.GetStream()); //first chance exception system.io.ioexception

                byte[] bytes = File.ReadAllBytes(file);
                Console.WriteLine(bytes.Length.ToString());
                sWriter.WriteLine(bytes.Length.ToString());
                sWriter.Flush();
                //receive OK
                byte[] rcv = new byte[1500];
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    if (String.Compare(rx.Substring(0, 2), "OK") == 0)

                        //send file
                        tcpclnt.Client.SendFile(file);
                }
            }
            catch
            {
                throw;
            }
        }

        //used for downloading a selected folder
        public void wrap_recv_zipfile(string f)
        {
            try
            {
                //todo metti in cartella tmp
                //TODO put everything in the same folder of the files downloaded
                f = f.Substring(f.LastIndexOf("\\"), f.Length - f.LastIndexOf("\\"));
                FileStream fStream = new FileStream(@"C:\Users\sds\Desktop\" + f + ".rar", FileMode.Create);

                // read the file in chunks of 1KB
                var buffer = new byte[1024];
                int bytesRead;

                //leggo la lunghezza
                bytesRead = tcpclnt.Client.Receive(buffer);
                string cmdFileSize = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                int length = Convert.ToInt32(cmdFileSize);

                int received = 0;

                while (received < length)
                {
                    bytesRead = tcpclnt.Client.Receive(buffer);
                    received += bytesRead;
                    if (received >= length)
                    {
                        bytesRead = bytesRead - (received - length);
                    }
                    fStream.Write(buffer, 0, bytesRead);

                    Console.WriteLine("ricevuti: " + bytesRead + " qualcosa XD");
                }
                fStream.Flush();
                fStream.Close();
                Console.WriteLine("File Ricevuto");
            }
            catch
            {
                throw;
            }
        }
         
        public void wrap_recv_file(string f)
        {
            try
            {
                //todo metti in cartella tmp
                f = Path.GetFileName(f);
                FileStream fStream = new FileStream(f, FileMode.Create);

                // read the file in chunks of 1KB
                var buffer = new byte[1024];
                int bytesRead;

                //leggo la lunghezza
                bytesRead = tcpclnt.Client.Receive(buffer);
                string cmdFileSize = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                int length = Convert.ToInt32(cmdFileSize);

                int received = 0;

                while (received < length)
                {
                    bytesRead = tcpclnt.Client.Receive(buffer);
                    received += bytesRead;
                    if (received >= length)
                    {
                        bytesRead = bytesRead - (received - length);
                    }
                    fStream.Write(buffer, 0, bytesRead);

                    Console.WriteLine("ricevuti: " + bytesRead + " qualcosa XD");
                }
                fStream.Flush();
                fStream.Close();
                Console.WriteLine("File Ricevuto");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void synchronize(string path2)
        {
            try
            {
                //login at every synch
                if (login(username, password) != 1)
                {
                    //TODO stampa qualche errore da qualche parte
                    throw new Exception();
                }
                //INVIO RICHIESTA SYNCH
                file_hash = new Dictionary<string, string>();
                string path = null;
                if (path2.StartsWith("C:\\"))
                {
                    int l = path2.Length - 3;
                    path = path2.Substring(3, l);
                }
                else
                {
                    path = path2;
                }

                byte[] credentials = Encoding.UTF8.GetBytes("S" + ":" + path);
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                //RICEVO OK
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                if (byteCount > 0)
                {
                    string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                    if (String.Compare(rx.Substring(0, 2), "OK") == 0)
                    {
                        //prendo il nome della cartella
                        // string path2 = rx.Substring(3, rx.Length - 3);

                        //AGGIUNGO C:\
                        string path1 = @"C:\";
                        string dir = path1 + path;
                        //creo il file
                        browse_folder_json(dir);
                        string json = JsonConvert.SerializeObject(file_hash);
                        credentials = Encoding.UTF8.GetBytes(json);
                        tcpclnt.Client.Send(credentials, SocketFlags.None);
                        //wrap_recv_file();
                        rcv = new byte[1500];
                        byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                        //TODO check if bytecount=0, remote host died
                        if (byteCount > 0)
                        {
                            //check if empty sendlist

                            rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                            if (String.Compare(rx.Substring(0, 3), "END") != 0)
                            {
                                List<string> to_send = JsonConvert.DeserializeObject<List<string>>(rx);

                                //INVIO I NUOVI FILE CHE MI VENGONO RICHIESTI
                                //read file line by line
                                //aggiungere il C://
                                foreach (var f in to_send)
                                {
                                    string newf = "C:\\" + f;
                                    wrap_send_file(newf);
                                    //ricevo qualcosa ad ogni invio

                                    if (!receive())
                                        break;
                                }
                                //TODO new view folder request, to update it
                                update_viewfolder = true;
                            }
                            //l'ultimo ok lo ricevo per forza
                            //TODO se va male mostrare qualcosa a schermo altrimenti dire che e' andato bene

                            // close socket after every synch
                            //if sendlist empty just close socket
                            tcpclnt.Client.Close();
                        }
                        else
                        {
                            Console.WriteLine("Something wrong. remote host died");
                        }
                    }
                }


                //TODO change it, this way is not good if some files less.
                //if (update_viewfolder == true)
                //{
                connect_to_server();
                List<string> items = new List<string>();
                items = view_folders();
                //ViewFolder.viewFolder.folders.Items.Clear();

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.GetType() == typeof(ViewFolder))
            {
                (window as ViewFolder).folders.Items.Clear();
                (window as ViewFolder).create_tree(items, (window as ViewFolder));
            }
        }
        //(Application.Current.MainWindow as ViewFolder).folders.Items.Clear();
        // Window owner = System.Windows.Application.Current.MainWindow;

        // Use owner here - it must be used on the UI thread as well..
        //ShowMyWindow(owner);
    }));

            }

            catch (Exception e)
            {
                throw;
            }
                //ViewFolder.viewFolder.create_tree(items); 
                
            //}
        }
        //return 1 if folder already present
        public List<string> view_folders(){
            try
            {
                if (login(username, password) != 1)
                {
                    //TODO stampa qualche errore da qualche parte
                    return null;
                }
                //view request
                byte[] credentials = Encoding.UTF8.GetBytes("V:" + username);

                tcpclnt.Client.Send(credentials, SocketFlags.None);

                //receive the list of folders, and the list of files in json. show them
                //TODO attenzione, 5000 byte potrebbero non bastare, vedere se c'è un modo dinamico per farlo
                byte[] rcv = new byte[5000];
                int byteCount = tcpclnt.Client.Receive(rcv, SocketFlags.None);
                string rx = (string)Encoding.UTF8.GetString(rcv).Clone();
                List<string> to_view = JsonConvert.DeserializeObject<List<string>>(rx);


                // close socket after every synch
                tcpclnt.Client.Close();
                return to_view;
            }
            catch
            {
                throw;
            }
            
        }
        //TODO change class in which this method is located, maybe add a folder class
        public long dirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += dirSize(di);
            }
            return size;
        }
        public void download_file(String f)
        {
            try
            {
                if (login(username, password) != 1)
                {
                    //todo stampa qualche errore da qualche parte
                    return;
                }
                //INVIO RICHIESTA SYNCH
                // string path = @"C:\Users\sds\Desktop\file.txt";
                byte[] credentials = Encoding.UTF8.GetBytes("D:" + f);
                byte[] rcv = new byte[1500];
                tcpclnt.Client.Send(credentials, SocketFlags.None);
                //TODO check if is a wrong message or not
                var ext = System.IO.Path.GetExtension(f);
                if (ext == String.Empty)
                {
                    //Its a directory
                    wrap_recv_zipfile(f);
                }

                else
                {
                    wrap_recv_file(f);
                }
                //todo se va male mostrare qualcosa a schermo altrimenti dire che e' andato bene



                // close socket after every synch
                tcpclnt.Client.Close();
            }
            catch
            {
                throw;
            }
        }
        /*
        //we will call this periodic method after we checked we already have something synchronized
        public async Task periodicSynchronization(TimeSpan dueTime, 
                                             TimeSpan interval,
                                             CancellationToken token,string path2)
        {
            
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);
            
            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Do some kind of work here. 
                Console.WriteLine("synchronization");
                {
                    connect_to_server();
                    synchronize(path2);
                }
                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
           
              }
    */
        
       
    }

    
}
