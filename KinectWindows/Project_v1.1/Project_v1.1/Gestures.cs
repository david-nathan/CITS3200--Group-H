using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace Project_v1._1
{
    [Serializable()]
    public class Gestures : ISerializable
    {

        public Dictionary<GestureKey, List<float[]>> gestures;

        public Gestures()
        {
            GestureKey gestureID = new GestureKey();
            List<float[]> gestureSequence = new List<float[]>();
            this.gestures = new Dictionary<GestureKey, List<float[]>>();
            gestures.Add(gestureID, gestureSequence);
        }

        //Deserialization Constructor
        public Gestures(SerializationInfo info, StreamingContext ctxt)
        {
            gestures = (Dictionary<GestureKey, List<float[]>>)info.GetValue("Gestures", typeof(Dictionary<GestureKey, List<float[]>>));

        }

        //Serialization Function
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Gestures", gestures);
        }

    }

    [Serializable()]
    public class GestureKey : ISerializable//, INotifyPropertyChanged  
    {
        public enum Rating { A, B, C, D, E, DEFAULT };
        public Rating rating { get; set; }

        public string name { get; set; }
        public DateTime recorded { get; set; }
        public int framenum { get; set; }
        public TimeSpan  timestamp { get; set; }

        public GestureKey()
        {
            rating = Rating.DEFAULT;
            name = "DEFAULT";
            recorded = DateTime.MinValue;
            framenum = 0;
            timestamp = new TimeSpan(0);
        }

        public GestureKey(Rating rating, string name, DateTime recorded, int framenum, TimeSpan timestamp)
        {
            this.rating = rating;
            this.name = name;
            this.recorded = recorded;
            this.framenum = framenum;
            this.timestamp = timestamp;
        }

        //Deserialzation Constructor
        public GestureKey(SerializationInfo info, StreamingContext ctxt)
        {
            rating = (Rating)info.GetValue("Rating", typeof(Rating));
            name = (String)info.GetValue("Name", typeof(String));
            recorded = (DateTime)info.GetValue("Recorded", typeof(DateTime));
            framenum = (int)info.GetValue("Frame Number", typeof(int));
            timestamp = (TimeSpan)info.GetValue("Time Stamp", typeof(TimeSpan));
        }

        //Serialization Function
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Rating", rating);
            info.AddValue("Name", name);
            info.AddValue("Recorded", recorded);
            info.AddValue("Frame Number", framenum);
            info.AddValue("Time Stamp", timestamp);
        }

    }
    
}
