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
        Rating rating;
        string name;
        DateTime recorded;

        public GestureKey()
        {
            rating = Rating.DEFAULT;
            name = "DEFAULT";
            recorded = DateTime.MinValue;
        }

        public GestureKey(Rating rating, string name, DateTime recorded)
        {
            this.rating = rating;
            this.name = name;
            this.recorded = recorded;
        }

        //Deserialzation Constructor
        public GestureKey(SerializationInfo info, StreamingContext ctxt)
        {
            rating = (Rating)info.GetValue("Rating", typeof(Rating));
            name = (String)info.GetValue("Name", typeof(String));
            recorded = (DateTime)info.GetValue("Recorded", typeof(DateTime));
        }

        //Serialization Function
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Rating", rating);
            info.AddValue("Name", name);
            info.AddValue("Recorded", recorded);
        }

    }
}
