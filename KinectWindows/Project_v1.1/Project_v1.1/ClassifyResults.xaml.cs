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
        public GestureKey.Rating prev_rating { get; set; }
        public TimeSpan prev_timespan { get; set; }
        public string result { get; set; }
        public GestureKey.Rating res_rating { get; set; }
        public TimeSpan res_timespan { get; set; }
        public string minDistance { get; set; }
        public GestureKey gKey { get; set; }
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
            prev_rating = GestureKey.Rating.DEFAULT;
            prev_timespan = new TimeSpan(0);
            result = "";
            prev_rating = GestureKey.Rating.DEFAULT;
            prev_timespan = new TimeSpan(0);
            minDistance = "";
            gKey = new GestureKey();
            save = false;
            
        }

        public Results(string recorded, string previous,GestureKey.Rating prev_rating, TimeSpan prev_timespan, string result,GestureKey.Rating res_rating, TimeSpan res_timespan, GestureKey gKey, string minDistance)
        {
            this.recorded = recorded;
            this.previous = previous;
            this.prev_rating = prev_rating;
            this.prev_timespan = prev_timespan;
            this.result = result;
            this.res_rating = res_rating;
            this.res_timespan = res_timespan;
            this.minDistance = minDistance;
            this.gKey = gKey;
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
