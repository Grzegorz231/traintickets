using Npgsql;
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
        DatabaseManager databaseManager = new DatabaseManager("localhost", 5432, "trains", "postgres", "2514");
        public SysAdminEnter()
        {
            InitializeComponent();
        }

        private void btnSysAdmOK_Click(object sender, RoutedEventArgs e)
        {
            databaseManager.OpenConnection();
            NpgsqlCommand commandToCheckAdmin = new NpgsqlCommand(@"SELECT employee_post, employee_login, employee_password
                                                                    FROM public.employee
                                                                    WHERE employee_post = 'Системный администратор' AND employee_login = @login", databaseManager.connection);    
            string login = txtBoxSysAdmLogin.Text;
            string password = txtBoxSysAdmPassword.Text;

            commandToCheckAdmin.Parameters.AddWithValue("@login", login);

            NpgsqlDataReader reader = commandToCheckAdmin.ExecuteReader();

            if (reader.Read())
            {
                string dbLogin, dbPassword;
                dbLogin = reader["employee_login"].ToString();
                dbPassword = reader["employee_password"].ToString();

                if(login == dbLogin && password == dbPassword)
                {
                    this.Close();
                    Registration registration = new Registration();
                    registration.Owner = this.Owner;
                    this.Owner.IsEnabled = false;
                    registration.Show();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }         
            }
            else
            {
                MessageBox.Show("Пользователь не найден!");
            }
            reader.Close();
            databaseManager.CloseConnection();
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
