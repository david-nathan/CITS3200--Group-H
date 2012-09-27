using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
    public class GestureKey : ISerializable
    {
        public enum Rating { A, B, C, D, E, DEFAULT };
        public string rating { get; set; }
        public string name { get; set; }
        public string recorded { get; set; }
        public string framenum { get; set; }
        public string timestamp { get; set; }

        public GestureKey()
        {
            rating = Rating.DEFAULT.ToString();
            name = "DEFAULT";
            recorded = DateTime.MinValue.ToShortDateString();
            framenum = "0";
            timestamp = "0";
        }

        public GestureKey(string rating, string name, string recorded, string framenum, string timestamp)
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
            rating = (String)info.GetValue("Rating", typeof(String));
            name = (String)info.GetValue("Name", typeof(String));
            recorded = (String)info.GetValue("Recorded", typeof(String));
            framenum = (String)info.GetValue("Frame Number", typeof(String));
            timestamp = (String)info.GetValue("Time Stamp", typeof(String));
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
