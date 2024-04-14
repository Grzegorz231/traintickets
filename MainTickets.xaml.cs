using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        private void menuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            isEditingMode = !isEditingMode;

            if (isEditingMode)
            {
                // Включаем функциональность перетаскивания элементов
                foreach (UIElement element in gridFarAway.Children)
                {
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                    element.MouseMove += Element_MouseMove;
                    element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                }
            }
            else
            {
                // Отключаем функциональность перетаскивания элементов
                foreach (UIElement element in gridFarAway.Children)
                {
                    element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                    element.MouseMove -= Element_MouseMove;
                    element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                }
            }
        }
        private bool isDragging = false;
        private Point startPoint;

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
                Point newPoint = e.GetPosition(null);
                Vector offset = startPoint - newPoint;

                ((UIElement)sender).SetValue(Canvas.LeftProperty, offset.X);
                ((UIElement)sender).SetValue(Canvas.TopProperty, offset.Y);

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
    }
}
