using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Project_v1._1
{
    class PlotGrid : INotifyPropertyChanged
    {
        public GestureKey gkey { get; set; }
        public GestureKey.Rating rating { get; set; }
        public string name { get; set; }
        public string recorded { get; set; }
        public int framenum { get; set; }
        public TimeSpan timespan { get; set; }
        private bool overlay;

        public bool Selected
        {
            get { return this.overlay; }
            set
            {
                if (this.overlay != value)
                {
                    this.overlay = value;
                    this.OnPropertyChanged("Selected");
                }
            }
        }

        public PlotGrid(GestureKey gkey, bool overlay)
        {
            this.gkey = gkey;
            this.rating = gkey.rating;
            this.name = gkey.name;
            this.recorded = gkey.recorded.ToString();
            this.framenum = gkey.framenum;
            this.timespan = gkey.timestamp;
            this.overlay = overlay;
        }




        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;


    }
}
