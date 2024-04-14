using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RailwayTickets
{
    public partial class MainTickets : Window
    {
        static string patronymicStatic = "";
        private bool isEditingMode = false;
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


        private void menuItemLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuItemSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool isDragging = false;
        private Point startPoint;

        private void menuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            isEditingMode = !isEditingMode;

            if (isEditingMode)
            {
                lblIsEditMode.Content = "Включен режим редактирования!";
                // Включаем функциональность перетаскивания элементов
                foreach (UIElement element in gridFarAway.Children)
                {
                    if (element is TextBox)
                    {
                        ((TextBox)element).IsReadOnly = true;
                        ((TextBox)element).Cursor = Cursors.Hand;
                        EnableTextBoxDragging((TextBox)element);
                    }
                    if (element is ComboBox)
                    {
                        EnableComboBoxDragging((ComboBox)element);
                    }
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                    element.MouseMove += Element_MouseMove;
                    element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                }
            }
            else
            {
                lblIsEditMode.Content = "Не в режиме редактирования";
                // Отключаем функциональность перетаскивания элементов
                foreach (UIElement element in gridFarAway.Children)
                {
                    if (element is TextBox)
                    {
                        ((TextBox)element).IsReadOnly = false;
                        ((TextBox)element).Cursor = Cursors.IBeam;
                        DisableTextBoxDragging((TextBox)element);
                    }
                    if (element is ComboBox)
                    {
                        DisableComboBoxDragging((ComboBox)element);
                    }
                    element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                    element.MouseMove -= Element_MouseMove;
                    element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
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
            if (isEditingMode && isDragging)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                UIElement element = sender as UIElement;

                double left = newPoint.X - startPoint.X + (double)element.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)element.GetValue(Canvas.TopProperty);

                element.SetValue(Canvas.LeftProperty, left);
                element.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = false;
                ((UIElement)sender).ReleaseMouseCapture();
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

        private void EnableComboBoxDragging(ComboBox comboBox)
        {
            comboBox.Cursor = Cursors.Hand; // Устанавливаем курсор для обозначения, что элемент можно перемещать
            comboBox.PreviewMouseLeftButtonDown += ComboBox_MouseLeftButtonDown;
            comboBox.PreviewMouseMove += ComboBox_MouseMove;
            comboBox.PreviewMouseLeftButtonUp += ComboBox_MouseLeftButtonUp;
        }

        private void DisableComboBoxDragging(ComboBox comboBox)
        {
            comboBox.Cursor = Cursors.Arrow; // Возвращаем обычный курсор
            comboBox.PreviewMouseLeftButtonDown -= ComboBox_MouseLeftButtonDown;
            comboBox.PreviewMouseMove -= ComboBox_MouseMove;
            comboBox.PreviewMouseLeftButtonUp -= ComboBox_MouseLeftButtonUp;
        }
        private ComboBox selectedComboBox;

        private void ComboBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                selectedComboBox = sender as ComboBox;
                selectedComboBox.CaptureMouse();
                e.Handled = true; // Предотвращаем начало выбора элемента в ComboBox
            }
        }

        private void ComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEditingMode && isDragging && selectedComboBox != null)
            {
                Point newPoint = e.GetPosition(gridFarAway);
                double left = newPoint.X - startPoint.X + (double)selectedComboBox.GetValue(Canvas.LeftProperty);
                double top = newPoint.Y - startPoint.Y + (double)selectedComboBox.GetValue(Canvas.TopProperty);

                selectedComboBox.SetValue(Canvas.LeftProperty, left);
                selectedComboBox.SetValue(Canvas.TopProperty, top);

                startPoint = newPoint;
            }
        }

        private void ComboBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditingMode && isDragging && selectedComboBox != null)
            {
                isDragging = false;
                selectedComboBox.ReleaseMouseCapture();
                selectedComboBox = null;
            }
        }

    }
}
