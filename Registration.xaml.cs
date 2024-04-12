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
    public partial class Registration : Window
    {
        static string patronymicStatic = "";
        public Registration()
        {
            InitializeComponent();
        }

        private void ChckBoxNoPatronymic_Checked(object sender, RoutedEventArgs e)
        {
            patronymicStatic = txtBoxRegPassportPatronymic.Text;
            txtBoxRegPassportPatronymic.Text = "";
            lblPassPatronymic.IsEnabled = false;
            txtBoxRegPassportPatronymic.IsEnabled=false;
        }

        private void ChckBoxNoPatronymic_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoxRegPassportPatronymic.Text = patronymicStatic;
            patronymicStatic = "";
            lblPassPatronymic.IsEnabled = true;
            txtBoxRegPassportPatronymic.IsEnabled = true;
        }

        private void btnRegAccept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRegCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.IsEnabled = true;
        }
    }
}
