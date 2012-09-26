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
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.Kinect;

namespace SkeletalProto
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class WindowPlot : Window
    {
        public WindowPlot()
        {
            InitializeComponent();
            drawPlot("JumpText.csv", 0, 0, 1);
        }

        public List<string[]> parseCSV(string path) //Parser for CSV file
        {
            List<string[]> parsedData = new List<string[]>();

            try
            {
                using (StreamReader readFile = new StreamReader(path))
                {
                    string line;
                    string[] row;

                    while ((line = readFile.ReadLine()) != null)
                    {
                        row = line.Split(',');
                        parsedData.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return parsedData;
        }

        private List<float[]> readInText(string path)
        {
            List<float[]> converted;
            List<string[]> str_gestureData = parseCSV(path);
            str_gestureData.RemoveAt(0); //Remove start date
            str_gestureData.RemoveAt(str_gestureData.Count-1); //Remove stop date
               
                converted = str_gestureData.ConvertAll(
                    new Converter<string[], float[]>(StringAtoFloatA)); //Convert to float
                return converted;
        }

        public static float[] StringAtoFloatA(string[] strA) //Converter for array conversion
        {
            float[] floatA = new float[strA.Length];
            for (int i = 0; i < strA.Length - 1; i++)
            {

                floatA[i] = float.Parse(strA[i]);
            }

            return floatA;
        }

        public void drawPlot(string path, int joint_index, int plane_x, int plane_y)
        {
            List<float[]> loaded;
            loaded = readInText(path);
            List<KeyValuePair<float,float>> refined = new List<KeyValuePair<float,float>>();
            foreach (float[] f in loaded)
            {
                refined.Add(new KeyValuePair<float, float>(f[(joint_index * 3 + plane_x + 2)], f[(joint_index * 3 + plane_y + 2)]));
            }
            scatter.DataContext = refined;

        }
    }
}
