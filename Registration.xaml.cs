using Npgsql;
using System;
using System.Windows;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;


namespace RailwayTickets
{
    public partial class Registration : Window
    {
        static string patronymicStatic = "";
        DatabaseManager databaseManager = new DatabaseManager("localhost", 5432, "trains", "postgres", "2514");

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
            try
            {
                databaseManager.OpenConnection();
                NpgsqlCommand commandToAddEmployee = new NpgsqlCommand(@"INSERT INTO public.employee (
                                                                        employee_id, 
                                                                        employee_passport, 
                                                                        employee_first_name, 
                                                                        employee_last_name, 
                                                                        employee_patronymic, 
                                                                        employee_post, 
                                                                        employee_login, 
                                                                        employee_password
                                                                    ) 
                                                                    SELECT 
                                                                        nextval('employee_id_sec'::regclass), 
                                                                        @passport_serie || ' ' || @passport_number, 
                                                                        @employee_name, 
                                                                        @employeeSurname, 
                                                                        @employeePatronymic, 
                                                                        'Кассир', 
                                                                        nextval('employee_login_id_sec') || '_' || @surname || '-' || @first_letter_name || '-' || @first_letter_patronymic, 
                                                                        @surname || '-' || @first_letter_name || '-' || @first_letter_patronymic
                                                                    ", databaseManager.connection);
                string transliteratedFirstName, transliteratedLastName;
                char firstLetterPatronymic, firstLetterName;
                string passSerie = txtBoxRegPassportSerie.Text;
                string passNumber = txtBoxRegPassportNum.Text;
                string firstName = txtBoxRegPassportName.Text;
                string lastName = txtBoxRegPassportLastName.Text;
                string patronymic = txtBoxRegPassportPatronymic.Text;

                if (string.IsNullOrEmpty(passSerie))
                {
                    throw new Exception("Поле с серией паспорта не может быть пустым!");
                }

                if (passSerie.Length < 4)
                {
                    throw new Exception("Серия паспорта не может иметь меньше четырёх цифр!");
                }

                if (string.IsNullOrEmpty(passNumber))
                {
                    throw new Exception("Поле с номером не может быть пустым!");
                }

                if (passNumber.Length < 6)
                {
                    throw new Exception("Номер паспорта не может иметь меньше шести цифр!");
                }

                if (string.IsNullOrEmpty(firstName))
                {
                    throw new Exception("Поле с именем не может быть пустым!");
                }

                if (string.IsNullOrEmpty(lastName))
                {
                    throw new Exception("Поле с фамилией не может быть пустым!");
                }
                

                transliteratedFirstName = Transliterate(firstName);
                firstLetterName = transliteratedFirstName[0];

                transliteratedLastName = Transliterate(lastName);
                firstLetterPatronymic = patronymic.Length > 0 ? Transliterate(patronymic)[0] : 'n';

                commandToAddEmployee.Parameters.AddWithValue("@passport_serie", passSerie);
                commandToAddEmployee.Parameters.AddWithValue("@passport_number", passNumber);
                commandToAddEmployee.Parameters.AddWithValue("@surname", transliteratedLastName);
                commandToAddEmployee.Parameters.AddWithValue("@first_letter_name", firstLetterName);
                commandToAddEmployee.Parameters.AddWithValue("@first_letter_patronymic", firstLetterPatronymic);
                commandToAddEmployee.Parameters.AddWithValue("@employee_name", firstName);
                commandToAddEmployee.Parameters.AddWithValue("@employeeSurname", lastName);
                commandToAddEmployee.Parameters.AddWithValue("@employeePatronymic", patronymic);

                if (txtBoxRegPassportSerie.Text.Length > 0 && txtBoxRegPassportNum.Text.Length > 0 && txtBoxRegPassportName.Text.Length > 0 && txtBoxRegPassportLastName.Text.Length > 0)
                {
                    try
                    {
                        commandToAddEmployee.ExecuteNonQuery();
                        MessageBox.Show("Пользователь успешно добавлен!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Пользователь с данными паспортными данными уже существует!");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                databaseManager.CloseConnection();
            }
            
        }

        private void btnRegCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void TxtBoxIsNumber(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.IsEnabled = true;
        }
        static string Transliterate(string Text)
        {
            Dictionary<char, string> transliterationMap = new Dictionary<char, string>
        {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"}, {'ё', "yo"},
            {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"},
            {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"}, {'у', "u"},
            {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"}, {'ш', "sh"}, {'щ', "shch"},
            {'ъ', ""}, {'ы', "y"}, {'ь', ""}, {'э', "e"}, {'ю', "yu"}, {'я', "ya"},
            {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"}, {'Е', "E"}, {'Ё', "Yo"},
            {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"}, {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"},
            {'Н', "N"}, {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"}, {'У', "U"},
            {'Ф', "F"}, {'Х', "Kh"}, {'Ц', "Ts"}, {'Ч', "Ch"}, {'Ш', "Sh"}, {'Щ', "Shch"},
            {'Ъ', ""}, {'Ы', "Y"}, {'Ь', ""}, {'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"}
        };

            string transliteratedText = "";

            foreach (char c in Text)
            {
                if (transliterationMap.ContainsKey(c))
                {
                    transliteratedText += transliterationMap[c];
                }
                else
                {
                    transliteratedText += c;
                }
            }

            return transliteratedText;
        }

        private void Txt(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
