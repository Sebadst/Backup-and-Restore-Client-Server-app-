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

namespace ProgettoPDS
{
    /// <summary>
    /// Logica di interazione per signup_window.xaml
    /// </summary>
    public partial class Signup_window : Window
    {
        public Signup_window()
        {
            InitializeComponent();
        }

        //FARE CLOSE SOCKET
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (username.Text == "" || password.Password == "" || repassword.Password == "")
                message.Content = "Non lasciare campi vuoti";
            else if (password.Password != repassword.Password)
                message.Content = "password diversa da ripeti password";
            else
            {
                message.Content = "Ok";
                //ora devo controllare se e' gia presente un utente con questo nome
                Client client = new Client(username.Text, password.Password);
                //DECOMMENTARE
                client.connect_to_server();
                //DECOMMENTARE
                int signup=client.signup(username.Text, password.Password);
                //int signup = 1;
                
                if (signup==1)
                {
                    //signup a buon fine
                    //apro finestra dell'utente
                  
                    MainWindow m = new MainWindow("Registrazione effettuata");
                   
                    m.Show();
                    /*
                    Home_window home_win = new Home_window(username.Text);
                    home_win.Show();
                    */
                    this.Close();
                }
                else if (signup == 0)
                {
                    //credenziali errate
                    //stampo un messaggio e do la possibilita di rifare il signup
                    message.Content = "L'utente e' gia' presente";
                }
                else
                {
                    //errore di connessione
                    message.Content = "Nessuna risposta dal server";
                }
                
               
            }

        }
    }       
}
