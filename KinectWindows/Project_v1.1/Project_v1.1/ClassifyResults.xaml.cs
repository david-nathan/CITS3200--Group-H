using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ClassifyResults.xaml
    /// </summary>
    public partial class ClassifyResults : Window
    {
        
       

        public ClassifyResults(List<Results> results)
        {       
            InitializeComponent();
            resultsListView.ItemsSource = results;
        }


        private void saveResults_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cancelResults_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }

    public class Results : INotifyPropertyChanged
    {
        public string recorded { get; set; }
        public string previous { get; set; }
        public string result { get; set; }
        public string minDistance { get; set; }
        private bool save;

        public bool Selected
        {
            get { return this.save; }
            set
            {
                if (this.save != value)
                {
                    this.save = value;
                    this.OnPropertyChanged("Selected");
                }
            }
        }

        public Results()
        {
            recorded = "";
            previous = "";
            result = "";
            minDistance = "";
            save = false;
            
        }

        public Results(string recorded, string previous, string result, string minDistance)
        {
            this.recorded = recorded;
            this.previous = previous;
            this.result = result;
            this.minDistance = minDistance;
            save = true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }


}
