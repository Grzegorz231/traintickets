using System.Windows;

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
