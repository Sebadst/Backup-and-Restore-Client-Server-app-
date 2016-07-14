using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;

namespace ProgettoPDS
{
    /// <summary>
    /// Logica di interazione per AddFolder.xaml
    /// </summary>
    public partial class AddFolder : Window
    {
        Client client;
        public AddFolder(Client client)
        {
            InitializeComponent();
            this.client = client;
            pbar.IsIndeterminate = false;
        }

        private void choose_folder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();



            // Get the selected file name and display in a TextBox 
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Open document 
                string filename = dialog.SelectedPath;

                path.Text = filename;
            }

        }
        //asynchronous logic
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
            var arg = (arguments)e.Argument; // to access elements ui from this thread
            (sender as BackgroundWorker).ReportProgress(0); //start pbar
            //put the operation here
            // string extractPath = @"C:\Users\sds\Desktop\progetto";
            //TODO change the path
            string zipPath = @"C:\Users\sds\Desktop\progetto\result.zip";
            //arg.path in place of startPath
            ZipFile.CreateFromDirectory(arg.path, zipPath);
            //ZipFile.ExtractToDirectory(zipPath, extractPath);

            //SEND THE ZIP FILE
            
                client.send_zip(arg.path, zipPath);    
                e.Result=1;
            }
            catch (Exception ex)
            {
                e.Result = -1;
            }
                //change window
            //commentato l'ultima volta

            
            //store the result
                        
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (e.UserState == null)
                pbar.IsIndeterminate = true;
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if ((int)e.Result == 1)
                {
                    //pbar.IsIndeterminate = false;
                    //TODO a check if everything ok. at this point we can open the new window
                    //we pass also path.text to be used in synchronization afterwards
                    ViewFolder view = new ViewFolder(client, path.Text);
                    view.Show();
                    this.Close();
                }
                else
                {
                    pbar.IsIndeterminate = false;
                    message.Content = "Errore, server non raggiungibile";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
       
        private void load_folder_Click(object sender, RoutedEventArgs e)
        {

            if (path.Text != "")
            {

                //  ZIP THE FILE
                string startPath = @path.Text;
                //check dimension<1gb
                if (client.dirSize(new DirectoryInfo(startPath)) > 1073741824)
                {
                    message.Content = "Non e' possibile caricare cartelle di dimensioni maggiori di 1 GB";
                }
                else
                {
                    try
                    {

                        //asynchronous
                        //asynchronous logic
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.WorkerReportsProgress = true;
                        worker.DoWork += worker_DoWork;
                        worker.ProgressChanged += worker_ProgressChanged;
                        worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                        var arg = new arguments() { path = startPath };
                        worker.RunWorkerAsync(arg);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.StackTrace);
                        message.Content = "Errore, impossibile contattare il server";
                    }
                   
                    //here I will call the periodic method
                    //var dueTime = TimeSpan.FromMinutes(1);
                    //var interval = TimeSpan.FromMinutes(1);
                }
            }
            // TODO: Add a CancellationTokenSource and supply the token here instead of None.
            //client.periodicSynchronization(dueTime,interval, CancellationToken.None,path.Text);

            //aggiunto per testare
          
            //DA DECOMMENTARE PER LA SYNCHRONIZE
            
            //client.connect_to_server();
            //client.synchronize(path.Text);
             
        }
    }
}
