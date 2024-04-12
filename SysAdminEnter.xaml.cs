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

namespace RailwayTickets
{
    public partial class SysAdminEnter : Window
    {
        public SysAdminEnter()
        {
            InitializeComponent();
        }

        private void btnSysAdmOK_Click(object sender, RoutedEventArgs e)
        {
            int aboba;
            aboba= 0;
        }

        private void btnSysAdmCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.IsEnabled = true;
            this.Close();        
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.IsEnabled = true;
        }
    }
}
