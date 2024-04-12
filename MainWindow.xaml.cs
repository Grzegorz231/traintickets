using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace RailwayTickets
{
    public partial class MainWindow : Window
    {
        DatabaseManager databaseManager = new DatabaseManager("localhost", 5432, "trains", "postgres", "2514");
        public MainWindow()
        {
            InitializeComponent();  
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            SysAdminEnter sysAdminEnter = new SysAdminEnter();
            sysAdminEnter.Owner = this;
            this.IsEnabled = false;
            sysAdminEnter.Show();
        }
    }
}
