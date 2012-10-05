using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Kinect;

namespace Project_v1._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        public Boolean recording { get; set; }
        System.IO.StreamWriter file;
        private Boolean firstframe = true;
        private Boolean kinectConnected = true;
        private DateTime startTime;
        private GestureKey selectedRow;
        private string start_time;
        private string gesture_type;
        private int initFrameNum;
        private long initTimeStamp;
        private string SaveFileLocation = @"C:\Users\Public";
        private string PlotDataLocation = @"C:\Users\Public";
        private Gestures gestureLibrary;
        //TO LOSE
        //private string startTime;





        public MainWindow()
        {
            InitializeComponent();
            tabItem1.IsEnabled = true;
            tabItem2.IsEnabled = true;
            recording = false;
            edit_button.IsEnabled = false;
            savegesture_button.IsEnabled = false;
            delete_button.IsEnabled = false;
            
            Stream stream = File.Open(@"C:\Users\Public\gestures.osl", FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            gestureLibrary = (Gestures)bformatter.Deserialize(stream);
            stream.Close();

            ratingCategory.ItemsSource = Enum.GetValues(typeof(GestureKey.Rating )).Cast<GestureKey.Rating>().AsEnumerable();
            dataGridGestures.ItemsSource = gestureLibrary.gestures.Keys.AsEnumerable();
            dataGridGestures.IsReadOnly = true;
                
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

            setKDButtons(true, true, false, false, false, false);

            if (KinectSensor.KinectSensors.Count == 0)
            {
                kinectConnected = false;
                setKDButtons(false, true, false, false, false, false);

            }
        }

        

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;

            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;

            if (newSensor == null)
            {
                setKDButtons(false, true, false, false, false, false);
                return;
            }

            //register for event and enable Kinect sensor features you want
            //newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
            newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newSensor.SkeletonStream.Enable();
            newSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(newSensor_SkeletonFrameReady);


            try
            {
                newSensor.Start();
                kinectConnected = true;
                setKDButtons(true, true, false, false, false, false);


            }
            catch (System.IO.IOException)
            {
                //another app is using Kinect
                kinectSensorChooser1.AppConflictOccurred();
            }

            //lblCurrentAngle.Content = kinectSensorChooser1.Kinect.ElevationAngle.ToString();
        }

        void newSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            SkeletonFrame skelFrame = e.OpenSkeletonFrame();

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {

                    //TODO: Throw up Error if more than one Skeleton Appears
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                }
            }
            if(skeletons.Length != 0)
            {
                foreach (Skeleton skel in skeletons)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (recording)
                        {
                            if (firstframe)
                            {
                                initFrameNum = skelFrame.FrameNumber;
                                initTimeStamp = skelFrame.Timestamp;
                                firstframe = false;
                            }

                            skelFrame.FrameNumber = skelFrame.FrameNumber - initFrameNum;
                            skelFrame.Timestamp = skelFrame.Timestamp - initTimeStamp;
                            WriteSkeleton writeSkel = new WriteSkeleton();
                            float[] data = writeSkel.WriteSkeletonToFile(skel, skelFrame, file); 
                        }
                    }
                }
            }

            if(skelFrame != null)
            {
            skelFrame.Dispose();
            }
        }

        //void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        //{
        //    
        //}

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }
                }
            }
        }

        private void setKDButtons(bool addToLib, bool targetFile, bool record, bool stop, bool classify, bool cancel)
        {
            addToLibraryButton.IsEnabled = addToLib;
            TargetFileButton.IsEnabled = targetFile;
            start_button.IsEnabled = record;
            stop_button.IsEnabled = stop;
            classify_button.IsEnabled = classify;
            cancelKDButton.IsEnabled = cancel;
        }

        private void cancelKDButton_Click(object sender, RoutedEventArgs e)
        {
            if (kinectConnected)
            {
                setKDButtons(true, true, false, false, false, false);
            }
            else
            {
                setKDButtons(false, true, false, false, false, false);
            }

            textBlock2.Text = "";

        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            button1.IsEnabled = false;

            //Set angle to slider1 value
            if (kinectSensorChooser1.Kinect != null && kinectSensorChooser1.Kinect.IsRunning)
            {
                kinectSensorChooser1.Kinect.ElevationAngle = (int)slider1.Value;
                lblCurrentAngle.Content = kinectSensorChooser1.Kinect.ElevationAngle;
            }

            //Do not change Elevation Angle often, please see documentation on this and Kinect Explorer for a robust example
            System.Threading.Thread.Sleep(new TimeSpan(hours: 0, minutes: 0, seconds: 1));
            button1.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser1.Kinect);

            //Save Gesture Libary 
            Stream stream = File.Open(@"C:\Users\Public\gestures.osl", FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, gestureLibrary);
            stream.Close();
        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
            recording = true;

            setKDButtons(false, false, false, true, false, true);
            file = new System.IO.StreamWriter(SaveFileLocation, true);
            string date = DateTime.Now.ToString();
            string str1 = "Start:, " + date + " , " + "Gesture:," + "_UNKNOWN";
            file.WriteLine(str1);
            string str = "Frame Number, Time Stamp,";
            Skeleton skeletonText = new Skeleton();
            foreach (Joint joint in skeletonText.Joints)
            {
                str = str + joint.JointType.ToString() + " X" + ","
                          + joint.JointType.ToString() + " Y" + ","
                          + joint.JointType.ToString() + " Z" + ",";
                
            }
            
            file.WriteLine(str);
        }

        private void stop_button_Click(object sender, RoutedEventArgs e)
        {
            
            
            string date = DateTime.Now.ToString();
            string str = "Stop:, " + date;
            file.WriteLine(str);
            file.Close();
            recording = false;
            firstframe = true;
            initFrameNum = 0;
            initTimeStamp = 0;


            setKDButtons(false, false, false, false, true, true);

            if ((String)(classify_button.Content) != "Save to Library")
            {
                setKDButtons(true, true, true, false, true, false);
            }    
        }

        private void targetFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            SaveFileDialog dlg = new SaveFileDialog();

            // Set filter for file extension and default file extension
            dlg.OverwritePrompt = false;
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Comma Separated Value File(.csv)|*.csv";
            dlg.InitialDirectory = SaveFileLocation;


            // Display SaveFileDialog by calling ShowDialog method
            DialogResult result = dlg.ShowDialog();


            // Get the selected file name
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //Save document
                SaveFileLocation = dlg.FileName;
                if (!dlg.FileName.EndsWith(".csv"))
                {
                    System.Windows.Forms.MessageBox.Show("Please select a filename with the '.csv' extension");

                }
                else
                {

                    textBlock2.Text = SaveFileLocation;
                    if (kinectConnected)
                    {
                        setKDButtons(false, true, true, false, true, true);
                    }
                    else
                    {
                        setKDButtons(false, true, false, false, true, true);
                    }
                }
            }


        }

        private void addToLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            start_button.IsEnabled = true;
            TargetFileButton.IsEnabled = false;
            classify_button.Content = "Save to Library";
            addToLibraryButton.IsEnabled = false;
            SaveFileLocation = @"C:\Users\Public\placeholder.csv";
        }

        private void classify_button_Click(object sender, RoutedEventArgs e)
        {
            TargetFileButton.IsEnabled = false;
            start_button.IsEnabled = false;
            Gestures sessionGestures;

            if (classify_button.Content.Equals("Save to Library"))
            {
                
                List<float[]> data = readInSingleGestureData(SaveFileLocation);

                GestureKey gkey = new GestureKey(GestureKey.Rating.DEFAULT, gesture_type, DateTime.Parse(start_time), (int)(data[data.Count-1][0]), new TimeSpan((long)(data[data.Count-1][1]*10000)));

                RecordGestureDialog dlg = new RecordGestureDialog();
                dlg.gestureNameTextBox.Text  = gkey.name;
                dlg.ratingComboBox.SelectedIndex = (int)(gkey.rating);
                dlg.textBlock1.Text = gkey.recorded.ToString();
                dlg.textBlock2.Text = gkey.timestamp.Seconds.ToString();
                dlg.textBlock3.Text = gkey.framenum.ToString();

                dlg.ShowDialog();

                if (dlg.DialogResult == true)
                {
                    gkey.name = dlg.gestureNameTextBox.Text;
                    gkey.rating = (GestureKey.Rating)(dlg.ratingComboBox.SelectedIndex);
                    gestureLibrary.gestures.Add(gkey, data);
                    File.Delete(SaveFileLocation);
                    SaveFileLocation = @"C:\Users\Public";
                    classify_button.Content = "Classify";
                    classify_button.IsEnabled = false;
                    TargetFileButton.IsEnabled = true;
                    addToLibraryButton.IsEnabled = true;
                    return;
                }
                else
                {
                    File.Delete(SaveFileLocation);
                    SaveFileLocation = @"C:\Users\Public";
                    classify_button.Content = "Classify";
                    classify_button.IsEnabled = false;
                    TargetFileButton.IsEnabled = true;
                    addToLibraryButton.IsEnabled = true;
                    return;
                }
                
            }


           
            string messageBoxText = "All Gestures in the current Save as file will be classified. Do you wish to continue?";
            string caption = "Classify Gestures";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBoxResult result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);

            if (result == MessageBoxResult.No)
            {
                return;
            }
             

            sessionGestures = readInSessionData(SaveFileLocation);
            DTWRecognition dtw = new DTWRecognition(sessionGestures, gestureLibrary, 0.6f, 2);

            Dictionary<GestureKey, Dictionary<GestureKey, List<float>>> results = dtw.classify();

            List<Results> table_results = new List<Results>(sessionGestures.gestures.Count);

            foreach (KeyValuePair<GestureKey, Dictionary<GestureKey, List<float>>> results_kvp in results)
            {
                GestureKey classification = new GestureKey();
                float minDistance = float.PositiveInfinity;

                foreach (KeyValuePair<GestureKey, List<float>> session_kvp in results_kvp.Value)
                {
                    if (session_kvp.Value[session_kvp.Value.Count - 1] < minDistance)
                    {
                        classification = session_kvp.Key;
                        minDistance = session_kvp.Value[session_kvp.Value.Count - 1];
                    }
                }
                Results input = new Results(results_kvp.Key.recorded.ToString(), results_kvp.Key.name, classification.name, minDistance.ToString());
                table_results.Add(input);
            }

            ClassifyResults dlgr = new ClassifyResults(table_results);
            
            dlgr.ShowDialog();

            if (dlgr.DialogResult == true)
            {
                foreach (Results save_result in dlgr.resultsListView.ItemsSource)
                {
                    //TODO: Write Back to CSV file.
                }
            }
            

        }


             private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
             {
                 if (e.Source.Equals(tabControl1))
                 {
                     if (tabControl1.SelectedIndex == 1)
                     {
                         dataGridGestures.ItemsSource = gestureLibrary.gestures.Keys.AsEnumerable();
                         setLibraryFields(-1, "", "");
                         setLibraryButtons(false, false, false);
                         selectedRow = null;
                     }
                 }
             }
     



        //========================================================================= Kinect Gestures Functions ==========================================================================================================


        private void dataGridGestures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridGestures.SelectedItem != null)
            {
                setLibraryFields(-1, "", "");
                selectedRow = (GestureKey)(dataGridGestures.SelectedItem);
                setLibraryButtons(true, true, false);            
            }           
        }

        private void gestureLibraryName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gestureLibraryName.Text == "")
            {
                setLibraryButtons(false, false, false);
            }       
        }

        private void gestureRatingCombo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gestureRatingCombo.SelectedIndex == -1)
            {
                setLibraryButtons(false, false, false);
            }         
        }

        private void edit_button_Click(object sender, RoutedEventArgs e)
        {

            setLibraryButtons(false, false, true);
            setLibraryFields((int)(selectedRow.rating), selectedRow.name, selectedRow.recorded.ToString());            
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Are you sure you want to delete the gesture\n\n" 
                                     + "  Name: "+ selectedRow.name + "\n" 
                                     + "  Recorded: " + selectedRow.recorded.ToString() + "\n\n"
                                     + " From the library?";
            string caption = "Delete Gesture";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBoxResult result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);

            if (result == MessageBoxResult.Yes)
            {
                gestureLibrary.gestures.Remove(selectedRow);
                dataGridGestures.Items.Refresh();
                setLibraryButtons(false, false, false);
            }
            else
            {
                return;
            }
        }

        private void savegesture_button_Click(object sender, RoutedEventArgs e)
        {
            if (gestureRatingCombo.SelectedIndex == -1 || gestureLibraryName.Text == "")
            {
                System.Windows.Forms.MessageBox.Show("Invalid Inputs");
                return;
            }
            else
            {
                KeyValuePair<GestureKey, List<float[]>> toEdit = new KeyValuePair<GestureKey,List<float[]>>();
                foreach (KeyValuePair<GestureKey, List<float[]>> kvp in gestureLibrary.gestures)
                {
                    if (selectedRow.Equals(kvp.Key))
                    {
                        toEdit = kvp;
                        break;
                    }
                }
                gestureLibrary.gestures.Remove(toEdit.Key);
                toEdit.Key.rating = (GestureKey.Rating)(gestureRatingCombo.SelectedIndex);
                toEdit.Key.name = gestureLibraryName.Text;
                gestureLibrary.gestures.Add(toEdit.Key, toEdit.Value);
                dataGridGestures.Items.Refresh();
                setLibraryFields(-1, "", "");
                setLibraryButtons(false, false, false);

            }
        }

        private void setLibraryFields(int comboIndex, string gestureName, string recordedDate)
        {           
            gestureRatingCombo.SelectedIndex =  comboIndex;
            gestureLibraryName.Text = gestureName;
            gestureLibraryRecord.Text = recordedDate;
        }

        private void setLibraryButtons(Boolean editIsEnabled, Boolean deleteIsEnabled, Boolean saveIsEnabled)
        {
            edit_button.IsEnabled = editIsEnabled;
            delete_button.IsEnabled = deleteIsEnabled;
            savegesture_button.IsEnabled = saveIsEnabled;

        }
      

//========================================================================= Kinect Plot Functions ==========================================================================================================


        private void tabItem2_Loaded(object sender, RoutedEventArgs e)
        {




        }

        private void loadPlotDatabutton2_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog dlg = new OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Comma Separated Value File(.csv)|*.csv";
            dlg.InitialDirectory = PlotDataLocation;


            // Display OpenFileDialog by calling ShowDialog method
            DialogResult result = dlg.ShowDialog();


            // Get the selected file name
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //Save document
                PlotDataLocation = dlg.FileName;
                if (!dlg.FileName.EndsWith(".csv"))
                {
                    System.Windows.Forms.MessageBox.Show("Please select a filename with the '.csv' extension");

                }
                else
                {
                    plotDatatextBlock.Text = PlotDataLocation;
                    Gestures sessionGestures = readInSessionData(PlotDataLocation);
                    ratingSessionCategory.ItemsSource = Enum.GetValues(typeof(GestureKey.Rating)).Cast<GestureKey.Rating>().AsEnumerable();
                    sessionGridGestures.ItemsSource = sessionGestures.gestures.Keys.AsEnumerable();
                    sessionGridGestures.IsReadOnly = true;

                }
            }

        }



        //========================================================================= Gesture from File ==========================================================================================================
        private List<float[]> readInSingleGestureData(string fileLocation)
        {
            List<float[]> float_gestureData;

            List<string[]> str_gestureData = parseCSV(fileLocation);
            
            start_time = str_gestureData[0][1];
            gesture_type = str_gestureData[0][3];

            str_gestureData.RemoveRange(0,2); //Remove start date
            str_gestureData.RemoveAt(str_gestureData.Count - 1); //Remove stop date

            float_gestureData = str_gestureData.ConvertAll(
                new Converter<string[], float[]>(StringAtoFloatA)); //Convert to float
            return float_gestureData;
        }

        private List<string[]> readInFullSession(string fileLocation)
        {
            List<string[]> str_sessionData = parseCSV(fileLocation);

            return str_sessionData;

        }


 
        private Gestures readInSessionData(string fileLocation)
        {
            Gestures sessionGestures = new Gestures();
            GestureKey.Rating _rating;
            string _name;
            DateTime _recorded;
            int _framenum;
            TimeSpan _timestamp;

            List<string[]> str_sessionData = readInFullSession(fileLocation);
            List<KeyValuePair<int, int>> gestureIndex = new List<KeyValuePair<int, int>>();

            if(str_sessionData[0][0] != "Start:")
            {
            System.Windows.Forms.MessageBox.Show("CSV file has incorrect format");
            return new Gestures();
            }

            int index = 0;

            for(int i=1; i<str_sessionData.Count; i++)
            {
                if (str_sessionData[i][0].Equals("Start:"))
                {
                    gestureIndex.Add(new KeyValuePair<int,int>(index, i-index));
                    index = i;
                }
            }

            gestureIndex.Add(new KeyValuePair<int,int>(index, str_sessionData.Count-1-index));

            List<KeyValuePair<int, int>>.Enumerator e = gestureIndex.GetEnumerator();

            while (e.MoveNext())
            {
                List<string[]> str_data = str_sessionData.GetRange(e.Current.Key, e.Current.Value);
                string[] firstline = str_data[0];
                str_data.RemoveRange(0,2);
                str_data.RemoveAt(str_data.Count - 1);
                List<float[]> float_data = str_data.ConvertAll(new Converter<string[], float[]>(StringAtoFloatA));

                _recorded = DateTime.Parse(firstline[1]);
                if (firstline[3].Equals("_UNKNOWN"))
                {
                    _name = firstline[3];
                    _rating = GestureKey.Rating.DEFAULT;
                }
                else
                {
                    _rating = (GestureKey.Rating)(firstline[3].ElementAt(firstline.Length - 1));
                    _name = firstline[3].Remove(firstline[3].Length - 2);
                }
                _framenum = (int)(float_data[float_data.Count - 1][0]);
                _timestamp = new TimeSpan((long)(float_data[float_data.Count - 1][1] * 10000));

                GestureKey dataKey = new GestureKey(_rating, _name, _recorded, _framenum, _timestamp);

                sessionGestures.gestures.Add(dataKey, float_data);
            }

           
            sessionGestures.gestures.Remove(sessionGestures.gestures.Keys.ElementAt(0));
            return sessionGestures;
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
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            return parsedData;
        }
































   



    }
}
