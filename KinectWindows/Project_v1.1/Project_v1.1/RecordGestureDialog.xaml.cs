using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_v1._1
{
    /// <summary>
    /// Interaction logic for RecordGestureDialog.xaml
    /// </summary>
    public partial class RecordGestureDialog : Window
    {
        public RecordGestureDialog()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }


}
