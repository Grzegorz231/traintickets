using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Windows.Markup;

namespace RailwayTickets
{
    public partial class MainTickets : Window
    {
        static string patronymicStatic = "";
        private bool isEditingMode = false;
        private bool isEditingModeFarAway = false;
        private bool isEditingModeNearCities = false;
        private bool isSelectedFarAway = true;
        private bool isSelectedNearCities = false;


        DatabaseManager databaseManager = new DatabaseManager("localhost", 5432, "trains", "postgres", "2514");

        public MainTickets()
        {
            InitializeComponent();
        }

        private void checkBoxNoPatronymic_Checked(object sender, RoutedEventArgs e)
        {
            patronymicStatic = txtBoxPatronymic.Text;
            txtBoxPatronymic.Text = "";
            lblPassengerPatronymic.IsEnabled = false;
            txtBoxPatronymic.IsEnabled=false;
        }

        private void checkBoxNoPatronymic_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoxPatronymic.Text = patronymicStatic;
            patronymicStatic = "";
            lblPassengerPatronymic.IsEnabled = true;
            txtBoxPatronymic.IsEnabled = true;
        }

        private void comboBoxNumPlace_DropDownOpened(object sender, EventArgs e)
        {
            databaseManager.OpenConnection();
            comboBoxNumPlace.Items.Clear();
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(@"SELECT place.place_number
                                                            FROM place
                                                            JOIN carriage ON carriage.carriage_id = place.carriage_id AND carriage.carriage_type = @carriageType
                                                            WHERE place.place_id NOT IN (SELECT place_id FROM ticket)", databaseManager.connection);

                command.Parameters.AddWithValue("@carriageType", comboBoxTrainType.Text);

                command.ExecuteNonQuery();
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    comboBoxNumPlace.Items.Add(reader["place_number"].ToString());
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            databaseManager.CloseConnection();
        }
        private void comboBoxStationTo_DropDownOpened(object sender, EventArgs e)
        {
            databaseManager.OpenConnection();
            comboBoxStationTo.Items.Clear();
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(@"SELECT station_name
                                                            FROM station", databaseManager.connection);

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    comboBoxStationTo.Items.Add(reader["station_name"].ToString());
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            databaseManager.CloseConnection();
        }

        private void comboBoxStationFrom_DropDownOpened(object sender, EventArgs e)
        {
            databaseManager.OpenConnection();
            comboBoxStationFrom.Items.Clear();
            try
            {
                NpgsqlCommand command = new NpgsqlCommand(@"SELECT station_name
                                                            FROM station", databaseManager.connection);

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    comboBoxStationFrom.Items.Add(reader["station_name"].ToString());
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            databaseManager.CloseConnection();
        }

        private void menuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                LoadInterface(filePath);
            }
        }
        private void LoadInterface(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                object obj = XamlReader.Load(fs);

                if (obj is TabControl)
                {
                    tabControl.Items.Clear();

                    TabControl loadedTabControl = (TabControl)obj;
                    foreach (object item in loadedTabControl.Items)
                    {
                        if (item is TabItem)
                        {
                            TabItem loadedTabItem = (TabItem)item;
                            TabItem newTabItem = new TabItem();
                            newTabItem.Header = loadedTabItem.Header;
                            newTabItem.Content = loadedTabItem.Content;

                            foreach (UIElement childElement in (loadedTabItem.Content as Panel).Children)
                            {
                                if (childElement is Button)
                                {
                                    Button button = childElement as Button;
                                    Button newButton = new Button();
                                    newButton.Content = button.Content;
                                    newButton.Click += button.Click;
                                    newTabItem.Content = newButton;
                                }
                            }

                            tabControl.Items.Add(newTabItem);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Неверный формат файла XAML.");
                }
            }
        }

        private void SaveInterface(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XamlWriter.Save(tabControl, fs);
            }
        }

        private void menuItemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                SaveInterface(filePath);
            }
        }

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                databaseManager.OpenConnection();
                // Добавляем пассажира в базу
                NpgsqlCommand commandToAddPassenger = new NpgsqlCommand(@"INSERT INTO public.passenger (
                                                                        passenger_id, 
                                                                        passenger_passport, 
                                                                        passenger_first_name, 
                                                                        passenger_last_name, 
                                                                        passenger_patronymic
                                                                    ) 
                                                                    SELECT 
                                                                        nextval('passengers_id_sec'::regclass), 
                                                                        @passport_serie || ' ' || @passport_number, 
                                                                        @passengerName, 
                                                                        @passengerSurname, 
                                                                        @passengerPatronymic
                                                                    WHERE NOT EXISTS (
                                                                        SELECT 1
                                                                        FROM public.passenger
                                                                        WHERE passenger_passport = @passport_serie || ' ' || @passport_number
                                                                    )
                                                                    ", databaseManager.connection);
                // Добавляем билет в базу
                NpgsqlCommand commandToAddTicket = new NpgsqlCommand(@"INSERT INTO public.ticket (
                                                                       ticket_id,
                                                                       passenger_id,
                                                                       station_id_from,
                                                                       station_id_to,
                                                                       cashbox_id,
                                                                       place_id,
                                                                       ticket_unique_number,
                                                                       ticket_date,
                                                                       ticket_time_from,
                                                                       ticket_time_to,
                                                                       ticket_total_time
                                                                    )
                                                                    SELECT
                                                                        nextval('ticket_id_sec'::regclass),
                                                                        passenger.passenger_id,
                                                                        @stationIdFrom,
                                                                        @stationIdTo,
                                                                        @cashboxId,
                                                                        (SELECT place_id 
                                                                         FROM place 
                                                                         JOIN carriage ON place.carriage_id = carriage.carriage_id
                                                                         WHERE carriage.carriage_type = @carriageType
                                                                         AND place.place_number = @placeNumber
                                                                         LIMIT 1),
                                                                        nextval('ticket_unique_number'::regclass),
                                                                        @ticketDate,
                                                                        '06:00'::TIME + (random() * '24 hours'::INTERVAL) as ticket_time_from,
                                                                        '06:00'::TIME + (random() * '24 hours'::INTERVAL) as ticket_time_to,
                                                                        ('06:00'::TIME + (random() * '24 hours'::INTERVAL)) - ('06:00'::TIME + (random() * '24 hours'::INTERVAL)) as ticket_total_time
                                                                    FROM public.passenger
                                                                    WHERE passenger.passenger_passport = @passport_serie || ' ' || @passport_number
                                                                    LIMIT 1;", databaseManager.connection);
                // Ищем id станции отправления
                NpgsqlCommand commandToGetStationIdFrom = new NpgsqlCommand(@"SELECT station_id 
                                                                              FROM public.station 
                                                                              WHERE station_name = @stationNameFrom", databaseManager.connection);
                // Ищем id станции назначения
                NpgsqlCommand commandToGetStationIdTo = new NpgsqlCommand(@"SELECT station_id 
                                                                            FROM public.station 
                                                                            WHERE station_name = @stationNameTo", databaseManager.connection);
                // Ищем место по номеру и типу вагона
                NpgsqlCommand commandToGetPlaceId = new NpgsqlCommand(@"SELECT place_id 
                                                                        FROM public.place 
                                                                        JOIN public.carriage ON place.carriage_id = carriage.carriage_id
                                                                        WHERE carriage.carriage_type = @carriageType
                                                                        AND place.place_number = @placeNumber
                                                                        LIMIT 1", databaseManager.connection);

                string passSerie = txtBoxPassSerie.Text;
                string passNumber = txtBoxPassNum.Text;
                string firstName = txtBoxName.Text;
                string lastName = txtBoxLastName.Text;
                string patronymic = txtBoxPatronymic.Text;
                string carriageType = comboBoxTrainType.Text;
                int stationIdFrom = 0, stationIdTo = 0, placeId = 0;
                string cashboxIdText = lblcashboxId.Content.ToString();
                int cashboxId = int.Parse(cashboxIdText.Split(':')[1].Trim());
         


                try
                {
                    commandToGetStationIdFrom.Parameters.AddWithValue("@stationNameFrom", comboBoxStationFrom.SelectedItem.ToString());
                }
                catch
                {
                    throw new Exception("Не выбрана станция отправления!");
                }
                NpgsqlDataReader reader = commandToGetStationIdFrom.ExecuteReader();
                if (reader.Read())
                {
                    string stationIdFromString  = reader["station_id"].ToString();
                    int.TryParse(stationIdFromString, out stationIdFrom);
                }
                reader.Close();



                try
                {
                    commandToGetStationIdTo.Parameters.AddWithValue("@stationNameTo", comboBoxStationTo.SelectedItem.ToString());
                }
                catch
                {
                    throw new Exception("Не выбрана станция назначения!");
                }
                reader = commandToGetStationIdTo.ExecuteReader();
                if (reader.Read())
                {
                    string stationIdToString = reader["station_id"].ToString();
                    int.TryParse(stationIdToString, out stationIdTo);
                }
                reader.Close();


               
                try
                {
                    commandToGetPlaceId.Parameters.AddWithValue("@carriageType", carriageType);
                }
                catch
                {
                    throw new Exception("Не выбран тип вагона!");
                }
                try
                {
                    commandToGetPlaceId.Parameters.AddWithValue("@placeNumber", comboBoxNumPlace.SelectedItem.ToString());
                }
                catch
                {
                    throw new Exception("Не выбран номер места!");
                }
                reader = commandToGetPlaceId.ExecuteReader();
                if (reader.Read())
                {
                    string placeIdString = reader["place_id"].ToString();
                    int.TryParse(placeIdString, out placeId);
                }
                reader.Close();



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

                // Добавляем данные для пассажира
                commandToAddPassenger.Parameters.AddWithValue("@passport_serie", passSerie);
                commandToAddPassenger.Parameters.AddWithValue("@passport_number", passNumber);
                commandToAddPassenger.Parameters.AddWithValue("@passengerName", firstName);
                commandToAddPassenger.Parameters.AddWithValue("@passengerSurname", lastName);
                commandToAddPassenger.Parameters.AddWithValue("@passengerPatronymic", patronymic);

                // Добавляем данные для билета
                commandToAddTicket.Parameters.AddWithValue("@passport_serie", passSerie);
                commandToAddTicket.Parameters.AddWithValue("@passport_number", passNumber);
                commandToAddTicket.Parameters.AddWithValue("@stationIdFrom", stationIdFrom);
                commandToAddTicket.Parameters.AddWithValue("@stationIdTo", stationIdTo);
                commandToAddTicket.Parameters.AddWithValue("@cashboxId", cashboxId);
                commandToAddTicket.Parameters.AddWithValue("@placeNumber", comboBoxNumPlace.SelectedItem.ToString());
                commandToAddTicket.Parameters.AddWithValue("@carriageType", comboBoxTrainType.Text);
                string ticketDateText = txtBoxDateFrom.Text;
                DateTime ticketDate;
                if (!DateTime.TryParseExact(ticketDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ticketDate))
                {
                    throw new Exception("Неверный формат даты. Дата должна быть в формате 'ГГГГ-ММ-ДД'.");
                }
                commandToAddTicket.Parameters.AddWithValue("@ticketDate", DateTime.Parse(txtBoxDateFrom.Text));



                if (txtBoxPassSerie.Text.Length > 0 && txtBoxPassNum.Text.Length > 0 && txtBoxName.Text.Length > 0 && txtBoxLastName.Text.Length > 0)
                {
                    try
                    {
                        commandToAddPassenger.ExecuteNonQuery();
                        commandToAddTicket.ExecuteNonQuery();
                        MessageBox.Show("Идёт печать...");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                databaseManager.CloseConnection();
            }
        }

        private bool isDragging = false;
        private Point startPoint;

        private void ShowAlignmentGuides(UIElement element)
        {
            ClearAlignmentGuides();
            List<UIElement> newElements = new List<UIElement>();

            double elementLeft = Canvas.GetLeft(element);
            double elementTop = Canvas.GetTop(element);
            double elementRight = elementLeft + element.RenderSize.Width;
            double elementBottom = elementTop + element.RenderSize.Height;

            foreach (UIElement sibling in gridFarAway.Children)
            {
                if (sibling != element)
                {
                    double siblingLeft = Canvas.GetLeft(sibling);
                    double siblingTop = Canvas.GetTop(sibling);
                    double siblingRight = siblingLeft + sibling.RenderSize.Width;
                    double siblingBottom = siblingTop + sibling.RenderSize.Height;

                    if (Math.Abs(elementLeft - siblingLeft) < 1)
                    {
                        Line verticalGuide = new Line();
                        verticalGuide.Stroke = Brushes.Red;
                        verticalGuide.X1 = elementLeft;
                        verticalGuide.X2 = siblingLeft;
                        verticalGuide.Y1 = Math.Min(elementTop, siblingTop);
                        verticalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(verticalGuide);
                    }

                    if (Math.Abs(elementTop - siblingTop) < 1)
                    {
                        Line horizontalGuide = new Line();
                        horizontalGuide.Stroke = Brushes.Red;
                        horizontalGuide.X1 = Math.Min(elementLeft, siblingLeft);
                        horizontalGuide.X2 = Math.Max(elementRight, siblingRight);
                        horizontalGuide.Y1 = elementTop;
                        horizontalGuide.Y2 = siblingTop;
                        newElements.Add(horizontalGuide);
                    }

                    if (Math.Abs(elementRight - siblingRight) < 1)
                    {
                        Line verticalGuide = new Line();
                        verticalGuide.Stroke = Brushes.Red;
                        verticalGuide.X1 = Math.Max(elementRight, siblingRight);
                        verticalGuide.X2 = Math.Max(elementRight, siblingRight);
                        verticalGuide.Y1 = Math.Min(elementTop, siblingTop);
                        verticalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(verticalGuide);
                    }

                    if (Math.Abs(elementBottom - siblingBottom) < 1)
                    {
                        Line horizontalGuide = new Line();
                        horizontalGuide.Stroke = Brushes.Red;
                        horizontalGuide.X1 = Math.Min(elementLeft, siblingLeft);
                        horizontalGuide.X2 = Math.Max(elementRight, siblingRight);
                        horizontalGuide.Y1 = Math.Max(elementBottom, siblingBottom);
                        horizontalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(horizontalGuide);
                    }
                }
            }
            foreach (UIElement newElement in newElements)
            {
                gridFarAway.Children.Add(newElement);
            }
        }
        private void ShowAlignmentGuidesNearCities(UIElement element)
        {
            ClearAlignmentGuidesNearCities();
            List<UIElement> newElements = new List<UIElement>();

            double elementLeft = Canvas.GetLeft(element);
            double elementTop = Canvas.GetTop(element);
            double elementRight = elementLeft + element.RenderSize.Width;
            double elementBottom = elementTop + element.RenderSize.Height;

            foreach (UIElement sibling in gridNearCities.Children)
            {
                if (sibling != element)
                {
                    double siblingLeft = Canvas.GetLeft(sibling);
                    double siblingTop = Canvas.GetTop(sibling);
                    double siblingRight = siblingLeft + sibling.RenderSize.Width;
                    double siblingBottom = siblingTop + sibling.RenderSize.Height;

                    if (Math.Abs(elementLeft - siblingLeft) < 1)
                    {
                        Line verticalGuide = new Line();
                        verticalGuide.Stroke = Brushes.Red;
                        verticalGuide.X1 = elementLeft;
                        verticalGuide.X2 = siblingLeft;
                        verticalGuide.Y1 = Math.Min(elementTop, siblingTop);
                        verticalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(verticalGuide);
                    }

                    if (Math.Abs(elementTop - siblingTop) < 1)
                    {
                        Line horizontalGuide = new Line();
                        horizontalGuide.Stroke = Brushes.Red;
                        horizontalGuide.X1 = Math.Min(elementLeft, siblingLeft);
                        horizontalGuide.X2 = Math.Max(elementRight, siblingRight);
                        horizontalGuide.Y1 = elementTop;
                        horizontalGuide.Y2 = siblingTop;
                        newElements.Add(horizontalGuide);
                    }

                    if (Math.Abs(elementRight - siblingRight) < 1)
                    {
                        Line verticalGuide = new Line();
                        verticalGuide.Stroke = Brushes.Red;
                        verticalGuide.X1 = Math.Max(elementRight, siblingRight);
                        verticalGuide.X2 = Math.Max(elementRight, siblingRight);
                        verticalGuide.Y1 = Math.Min(elementTop, siblingTop);
                        verticalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(verticalGuide);
                    }

                    if (Math.Abs(elementBottom - siblingBottom) < 1)
                    {
                        Line horizontalGuide = new Line();
                        horizontalGuide.Stroke = Brushes.Red;
                        horizontalGuide.X1 = Math.Min(elementLeft, siblingLeft);
                        horizontalGuide.X2 = Math.Max(elementRight, siblingRight);
                        horizontalGuide.Y1 = Math.Max(elementBottom, siblingBottom);
                        horizontalGuide.Y2 = Math.Max(elementBottom, siblingBottom);
                        newElements.Add(horizontalGuide);
                    }
                }
            }
            foreach (UIElement newElement in newElements)
            {
                gridNearCities.Children.Add(newElement);
            }
        }
        private void ClearAlignmentGuidesNearCities()
        {
            List<UIElement> guidesToRemove = new List<UIElement>();
            foreach (UIElement child in gridNearCities.Children)
            {
                if (child is Line)
                {
                    guidesToRemove.Add(child);
                }
            }

            foreach (UIElement guide in guidesToRemove)
            {
                gridNearCities.Children.Remove(guide);
            }
        }

        private void ClearAlignmentGuides()
        {
            List<UIElement> guidesToRemove = new List<UIElement>();
            foreach (UIElement child in gridFarAway.Children)
            {
                if (child is Line)
                {
                    guidesToRemove.Add(child);
                }
            }

            foreach (UIElement guide in guidesToRemove)
            {
                gridFarAway.Children.Remove(guide);
            }
        }

        private void menuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            isEditingMode = !isEditingMode;
            if (isSelectedFarAway)
            {
                if (isEditingMode)
                {
                    lblIsEditMode.Content = "Включен режим редактирования!";
                    foreach (UIElement element in gridFarAway.Children)
                    {
                        if (element is TextBox)
                        {
                            ((TextBox)element).IsReadOnly = true;
                            ((TextBox)element).Cursor = Cursors.Hand;
                            EnableTextBoxDragging((TextBox)element);
                        }
                        if (element is CheckBox)
                        {
                            EnableCheckBoxDragging((CheckBox)element);
                        }
                        if (element is Button)
                        {
                            EnableButtonDragging((Button)element);
                        }
                        if (element is Viewbox)
                        {
                            ((Viewbox)element).Cursor = Cursors.Hand;
                        }

                        element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                        element.MouseMove += Element_MouseMove;
                        element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                    }
                }
                else
                {
                    ClearAlignmentGuides();
                    lblIsEditMode.Content = "";
                    foreach (UIElement element in gridFarAway.Children)
                    {
                        if (element is TextBox)
                        {
                            ((TextBox)element).IsReadOnly = false;
                            ((TextBox)element).Cursor = Cursors.IBeam;
                            DisableTextBoxDragging((TextBox)element);
                        }
                        if (element is CheckBox)
                        {
                            DisableCheckBoxDragging((CheckBox)element);
                        }
                        if (element is Button)
                        {
                            DisableButtonDragging((Button)element);
                        }
                        if (element is Viewbox)
                        {
                            ((Viewbox)element).Cursor = Cursors.Arrow;
                        }
                        element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                        element.MouseMove -= Element_MouseMove;
                        element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                    }
                }
            }
            else if (isSelectedNearCities)
            {
                if (isEditingMode)
                {
                    lblIsEditingNearCities.Content = "Включен режим редактирования!";
                    foreach (UIElement element in gridNearCities.Children)
                    {
                        if (element is TextBox)
                        {
                            ((TextBox)element).IsReadOnly = true;
                            ((TextBox)element).Cursor = Cursors.Hand;
                            EnableTextBoxDragging((TextBox)element);
                        }
                        if (element is CheckBox)
                        {
                            EnableCheckBoxDragging((CheckBox)element);
                        }
                        if (element is Button)
                        {
                            EnableButtonDragging((Button)element);
                        }
                        if (element is Viewbox)
                        {
                            ((Viewbox)element).Cursor = Cursors.Hand;
                        }

                        element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                        element.MouseMove += Element_MouseMove;
                        element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                    }
                }
                else
                {
                    ClearAlignmentGuidesNearCities();
                    lblIsEditingNearCities.Content = "";
                    foreach (UIElement element in gridNearCities.Children)
                    {
                        if (element is TextBox)
                        {
                            ((TextBox)element).IsReadOnly = false;
                            ((TextBox)element).Cursor = Cursors.IBeam;
                            DisableTextBoxDragging((TextBox)element);
                        }
                        if (element is CheckBox)
                        {
                            DisableCheckBoxDragging((CheckBox)element);
                        }
                        if (element is Button)
                        {
                            DisableButtonDragging((Button)element);
                        }
                        if (element is Viewbox)
                        {
                            ((Viewbox)element).Cursor = Cursors.Arrow;
                        }
                        element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                        element.MouseMove -= Element_MouseMove;
                        element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                    }
                }
            }
            
        }

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEditingMode && isDragging && isSelectedFarAway)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                UIElement element = sender as UIElement;

                double left = newPoint.X - startPoint.X + (double)element.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)element.GetValue(Canvas.TopProperty);

                element.SetValue(Canvas.LeftProperty, left);
                element.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;

                ShowAlignmentGuides(element);
            }
            if(isEditingMode && isDragging && isSelectedNearCities)
            {
                Point newPoint = e.GetPosition(gridNearCities);
                UIElement element = sender as UIElement;

                double left = newPoint.X - startPoint.X + (double)element.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)element.GetValue(Canvas.TopProperty);

                element.SetValue(Canvas.LeftProperty, left);
                element.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;

                ShowAlignmentGuidesNearCities(element);
            }
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = false;
                ((UIElement)sender).ReleaseMouseCapture();
                ClearAlignmentGuides();
            }
        }

        private void EnableTextBoxDragging(TextBox textBox)
        {
            textBox.PreviewMouseLeftButtonDown += TextBox_MouseLeftButtonDown;
            textBox.PreviewMouseMove += TextBox_MouseMove;
            textBox.PreviewMouseLeftButtonUp += TextBox_MouseLeftButtonUp;
        }

        private void DisableTextBoxDragging(TextBox textBox)
        {
            textBox.PreviewMouseLeftButtonDown -= TextBox_MouseLeftButtonDown;
            textBox.PreviewMouseMove -= TextBox_MouseMove;
            textBox.PreviewMouseLeftButtonUp -= TextBox_MouseLeftButtonUp;
        }

        private TextBox selectedTextBox;

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //вызывается при нажатии мыши, чтобы "взять" textbox
        {
            if (isEditingMode)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                selectedTextBox = sender as TextBox;
                selectedTextBox.CaptureMouse();
                e.Handled = true;
            }
        }

        private void TextBox_MouseMove(object sender, MouseEventArgs e) //вызывается при перемещении мыши, когда "взят" textbox
        {
            if (isEditingMode && isDragging && selectedTextBox != null)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                double left = newPoint.X - startPoint.X + (double)selectedTextBox.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)selectedTextBox.GetValue(Canvas.TopProperty);

                selectedTextBox.SetValue(Canvas.LeftProperty, left);
                selectedTextBox.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
        }

        private void TextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //вызывается при поднятии мыши и устанавливает конечное положение textbox'a
        {
            if (isEditingMode && isDragging && selectedTextBox != null)
            {
                isDragging = false;
                selectedTextBox.ReleaseMouseCapture();
                selectedTextBox = null;
            }
        }
        private void EnableCheckBoxDragging(CheckBox checkBox)
        {
            checkBox.Cursor = Cursors.Hand;
            checkBox.PreviewMouseLeftButtonDown += CheckBox_MouseLeftButtonDown;
            checkBox.PreviewMouseMove += CheckBox_MouseMove;
            checkBox.PreviewMouseLeftButtonUp += CheckBox_MouseLeftButtonUp;
        }

        private void DisableCheckBoxDragging(CheckBox checkBox)
        {
            checkBox.Cursor = Cursors.Arrow;
            checkBox.PreviewMouseLeftButtonDown -= CheckBox_MouseLeftButtonDown;
            checkBox.PreviewMouseMove -= CheckBox_MouseMove;
            checkBox.PreviewMouseLeftButtonUp -= CheckBox_MouseLeftButtonUp;
        }
        private CheckBox selectedCheckBox;

        private void CheckBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                selectedCheckBox = sender as CheckBox;
                selectedCheckBox.CaptureMouse();
                e.Handled = true;
            }
        }

        private void CheckBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEditingMode && isDragging && selectedCheckBox != null)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                double left = newPoint.X - startPoint.X + (double)selectedCheckBox.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)selectedCheckBox.GetValue(Canvas.TopProperty);

                selectedCheckBox.SetValue(Canvas.LeftProperty, left);
                selectedCheckBox.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
        }

        private void CheckBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode && isDragging && selectedCheckBox != null)
            {
                isDragging = false;
                selectedCheckBox.ReleaseMouseCapture();
                selectedCheckBox = null;
            }
        }

        private void EnableButtonDragging(Button button)
        {
            button.Cursor = Cursors.Hand;
            button.PreviewMouseLeftButtonDown += Button_MouseLeftButtonDown;
            button.PreviewMouseMove += Button_MouseMove;
            button.PreviewMouseLeftButtonUp += Button_MouseLeftButtonUp;
        }

        private void DisableButtonDragging(Button button)
        {
            button.Cursor = Cursors.Arrow;
            button.PreviewMouseLeftButtonDown -= Button_MouseLeftButtonDown;
            button.PreviewMouseMove -= Button_MouseMove;
            button.PreviewMouseLeftButtonUp -= Button_MouseLeftButtonUp;
        }
        private Button selectedButton;

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                selectedButton = sender as Button;
                selectedButton.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEditingMode && isDragging && selectedButton != null && isSelectedFarAway)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                double left = newPoint.X - startPoint.X + (double)selectedButton.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)selectedButton.GetValue(Canvas.TopProperty);

                selectedButton.SetValue(Canvas.LeftProperty, left);
                selectedButton.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
            if (isEditingMode && isDragging && selectedButton != null && isSelectedNearCities)
            {
                Point newPoint = e.GetPosition(gridNearCities);
                double left = newPoint.X - startPoint.X + (double)selectedButton.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)selectedButton.GetValue(Canvas.TopProperty);

                selectedButton.SetValue(Canvas.LeftProperty, left);
                selectedButton.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode && isDragging && selectedButton != null)
            {
                isDragging = false;
                selectedButton.ReleaseMouseCapture();
                selectedButton = null;
            }
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl != null && tabControl.SelectedItem == tabItemNearCities)
            {
                isSelectedNearCities = true;
                isSelectedFarAway = false;
                lblIsEditMode.Content = "";
                lblIsEditingNearCities.Content = "";
                isEditingMode = false;
                isEditingModeFarAway = false;
                isEditingModeNearCities = false;
            }
            if(tabControl != null && tabControl.SelectedItem == tabItemFarAway)
            {
                isSelectedNearCities = false;
                isSelectedFarAway = true;
                lblIsEditMode.Content = "";
                lblIsEditingNearCities.Content = "";
                isEditingMode= false;
                isEditingModeFarAway = false;
                isEditingModeNearCities = false;
            }
        }

        private void txtBoxPassSerie_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
