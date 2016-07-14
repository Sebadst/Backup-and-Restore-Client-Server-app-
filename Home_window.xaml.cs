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
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
namespace ProgettoPDS
{
    /// <summary>
    /// Logica di interazione per Home_window.xaml
    /// </summary>
    public partial class Home_window : Window
    {
        Client client;
        public Home_window(string username,Client client)
        {
            InitializeComponent();
            welcome_label.Content="Welcome "+username;
            this.client = client;
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

       
        private void add_folder(object sender, RoutedEventArgs e)
        {
            MyContentControl.Content = new AddFolder(client);
        }
       /*
        private void view_folder(object sender, RoutedEventArgs e)
        {
            MyContentControl.Content = new ViewFolder(client);
        }
        * */
        void send_md5()
        {

        }
        void calculate_md5()
        {

        }
       
    }
}
