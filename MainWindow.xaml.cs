using Npgsql;
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
            databaseManager.OpenConnection();
            NpgsqlCommand commandToCheckUser = new NpgsqlCommand(@"SELECT employee_login, employee_password, cashbox_id
                                                                    FROM public.employee
                                                                    JOIN cashbox ON cashbox.employee_id = employee.employee_id
                                                                    WHERE employee_login = @login", databaseManager.connection);
            string login = txtBoxLogin.Text;
            string password = txtBoxPassword.Text;

            commandToCheckUser.Parameters.AddWithValue("@login", login);

            NpgsqlDataReader reader = commandToCheckUser.ExecuteReader();

            if (reader.Read())
            {
                string dbLogin, dbPassword;
                dbLogin = reader["employee_login"].ToString();
                dbPassword = reader["employee_password"].ToString();
                string cashboxId = "";
                cashboxId = reader["cashbox_id"].ToString();

                if (login == dbLogin && password == dbPassword)
                {         
                    MainTickets mainTickets = new MainTickets();
                    mainTickets.Show();
                    mainTickets.cashboxId.Content = "Номер кассы: " + cashboxId;
                    mainTickets.lblCashboxIdNearcity.Content = "Номер кассы: " + cashboxId;
                    this.Close();
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

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            SysAdminEnter sysAdminEnter = new SysAdminEnter();
            sysAdminEnter.Owner = this;
            this.IsEnabled = false;
            sysAdminEnter.Show();
        }
    }
}
