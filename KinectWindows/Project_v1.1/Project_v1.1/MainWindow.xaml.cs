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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using Microsoft.Xna.Framework;

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
        private PlotGrid selectedPlotRow;
        private string start_time;
        private string gesture_type;
        private int initFrameNum;
        private long initTimeStamp;
        private string SaveFileLocation = @"C:\Users\Public";
        private string PlotDataLocation = @"C:\Users\Public";
        private Gestures gestureLibrary;
        private Gestures sessionGestures;

        //Boolean variables to enable the 'Plot' button
        private Boolean isFileloaded = false;
        private Boolean isRowSelected = false;
        private Boolean isPlotterSelected = false;
        private Boolean isBodySegmentSelected = false;

        //Used to clear plotter
        List<LineAndMarker<ElementMarkerPointsGraph>> chart;
        List<LineAndMarker<ElementMarkerPointsGraph>> chart2;
        //TO LOSE
        //private string startTime;





        public MainWindow()
        {
            InitializeComponent();
            tabItem1.IsEnabled = true;
            tabItem2.IsEnabled = true;
            gesturesTabItem.IsEnabled = true;
            recording = false;
            edit_button.IsEnabled = false;
            TargetFileButton.IsEnabled = false;
            savegesture_button.IsEnabled = false;
            delete_button.IsEnabled = false;
            
            Stream stream = File.Open(@"gestures.osl", FileMode.Open);
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
            Stream stream = File.Open(@"gestures.osl", FileMode.Open);
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

            if (classify_button.Content.Equals("Save to Library"))
            {
                
                List<float[]> data = readInSingleGestureData(SaveFileLocation);

                GestureKey gkey = new GestureKey(GestureKey.Rating.DEFAULT, gesture_type, DateTime.Parse(start_time), (int)(data[data.Count-1][0]), new TimeSpan((long)(data[data.Count-1][1]*10000)));

                RecordGestureDialog dlg = new RecordGestureDialog();
                dlg.gestureNameTextBox.Text  = gkey.name;
                dlg.ratingComboBox.SelectedIndex = (int)(gkey.rating);
                dlg.textBlock1.Text = gkey.recorded.ToString();
                dlg.textBlock2.Text = gkey.timestamp.ToString();
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
                    cancelKDButton.IsEnabled = false;
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
                    cancelKDButton.IsEnabled = false;
                    TargetFileButton.IsEnabled = true;
                    addToLibraryButton.IsEnabled = true;
                    return;
                }
                
            }


           
            string messageBoxText = "All Gestures in the current Target File will be classified.\n Do you wish to continue?";
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
                Results input = new Results(
                    results_kvp.Key.recorded.ToString(), 
                    results_kvp.Key.name, 
                    results_kvp.Key.rating,
                    results_kvp.Key.timestamp,
                    classification.name,
                    classification.rating,
                    classification.timestamp,
                    results_kvp.Key,
                    minDistance.ToString());

                table_results.Add(input);
            }

            ClassifyResults dlgr = new ClassifyResults(table_results);
            
            dlgr.ShowDialog();

            if (dlgr.DialogResult == true)
            {
                foreach (Results save_result in dlgr.resultsListView.ItemsSource)
                {
                    if (save_result.Selected)
                    {
                        List<float[]> data = sessionGestures.gestures[save_result.gKey];
                        sessionGestures.gestures.Remove(save_result.gKey);
                        save_result.gKey.name = save_result.result;
                        save_result.gKey.rating = save_result.res_rating;
                        sessionGestures.gestures.Add(save_result.gKey, data);
                    }                  
                }

                //Write Back to CSV file.
                writeToFile(SaveFileLocation, sessionGestures);
            }

            if (kinectConnected)
            {
                setKDButtons(false, true, true, false, true, true);
            }
            else
            {
                setKDButtons(false, true, false, false, true, true);
            }
           
        }


             private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
             {
                 if (e.Source.Equals(tabControl1))
                 {
                     if (tabControl1.SelectedIndex == 1)
                     {
                         dataGridGestures.Items.Refresh();
                         setLibraryFields(-1, "", "");
                         setLibraryButtons(false, false, false);
                         selectedRow = null;
                     }
                 }
             }
     



 //========================================================================= Kinect Gestures Functions ==========================================================================================================


        private void gesturesTabItem_Loaded(object sender, RoutedEventArgs e)
        {
		    dataGridGestures.ItemsSource = gestureLibrary.gestures.Keys.AsEnumerable();
			setLibraryFields(-1, "", "");
			setLibraryButtons(false, false, false);
			selectedRow = null;
	    }

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
                    sessionGestures = readInSessionData(PlotDataLocation);
                    ratingSessionCategory.ItemsSource = Enum.GetValues(typeof(GestureKey.Rating)).Cast<GestureKey.Rating>().AsEnumerable();
                    List<PlotGrid> sessgridData = new List<PlotGrid>();
                    foreach (GestureKey gkey in sessionGestures.gestures.Keys)
                    {
                        sessgridData.Add(new PlotGrid(gkey, false));
                    }
                    sessionGridGestures.ItemsSource = sessgridData;
                    isFileloaded = true;
                    Plot_Type_ComboBox.IsEnabled = true;
                    Check_For_PlotButton();
                }
            }

        }

        private void Plot_Type_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Body_Segment_ComboBox.IsEnabled = true;

            if (Plot_Type_ComboBox.SelectedIndex == 0 || Plot_Type_ComboBox.SelectedIndex == 1)
            {
                //Disable non-angle body segments
                CBox_item_hipCentre.IsEnabled = true;
                CBox_item_shoulderCentre.IsEnabled = true;
                CBox_item_head.IsEnabled = true;
                CBox_item_handLeft.IsEnabled = true;
                CBox_item_handRight.IsEnabled = true;
                CBox_item_footLeft.IsEnabled = true;
                CBox_item_footRight.IsEnabled = true;

                if (Plot_Type_ComboBox.SelectedIndex == 0)
                {
                    //Set Radio buttons appropriately
                    Radio_optionA.Content = "x-y axis";
                    Radio_optionB.Content = "x-z axis";
                    Radio_optionA.IsEnabled = true;
                    Radio_optionB.IsEnabled = true;

                    Radio_optionA.IsChecked = true;

                    xyz.Opacity = 0;
                }
                else
                {
                    //Set Radio buttons appropriately
                    Radio_optionA.Content = "meters";
                    Radio_optionB.Content = "meters per second";
                    Radio_optionA.IsEnabled = true;
                    Radio_optionB.IsEnabled = true;

                    Radio_optionA.IsChecked = true;

                    //Add appropriate combo box elements
                    xyz.Opacity = 100;
                    Radio_X.IsChecked = true;
                }
            }
            else if (Plot_Type_ComboBox.SelectedIndex == 2)
            {
                //Set Radio buttons appropriately
                Radio_optionA.Content = "degrees";
                Radio_optionB.Content = "degrees per second";
                Radio_optionA.IsEnabled = true;
                Radio_optionB.IsEnabled = true;

                Radio_optionA.IsChecked = true;

                xyz.Opacity = 0;

                //Disable non-angle body segments
                CBox_item_hipCentre.IsEnabled = false;
                CBox_item_shoulderCentre.IsEnabled = false;
                CBox_item_head.IsEnabled = false;
                CBox_item_handLeft.IsEnabled = false;
                CBox_item_handRight.IsEnabled = false;
                CBox_item_footLeft.IsEnabled = false;
                CBox_item_footRight.IsEnabled = false;

            }

        }

        private void Body_Segment_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isPlotterSelected = true;
            isBodySegmentSelected = true;
            Check_For_PlotButton();
        }
        
        private void sessionGridGestures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sessionGridGestures.SelectedItem != null)
            {
                selectedPlotRow = (PlotGrid)(sessionGridGestures.SelectedItem);
                isRowSelected = true;
                Check_For_PlotButton();
            }
            else
            {
                isRowSelected = false;
            }
        }

        private void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            //plotter = new ChartPlotter();
            //plotter.Viewport.RenderSize = 1;
            //plotter.Viewport.Zoom(0.00001);

            plotter.Children.RemoveAll(typeof(LineGraph));
            foreach (LineAndMarker<ElementMarkerPointsGraph> p in chart)
            {
                p.MarkerGraph.DataSource = null;
            }

            if (Plot_Type_ComboBox.SelectedIndex == 0)
            {
                foreach (LineAndMarker<ElementMarkerPointsGraph> p in chart2)
                {
                    p.MarkerGraph.DataSource = null;
                }
            }
            Clear_button.IsEnabled = false;
            Clear_button.Content = "";
            Body_Segment_ComboBox.SelectedIndex = -1;
            isBodySegmentSelected = false;

            foreach (PlotGrid pg in sessionGridGestures.ItemsSource)
            {
                pg.Selected = false;
            }

            // enable combo boxes and radio buttons 
            sessionGridGestures.IsEnabled = true;
            loadPlotDatabutton2.IsEnabled = true;
            Plot_Type_ComboBox.IsEnabled = true;
            Radio_Grid.IsEnabled = true;
            Body_Segment_ComboBox.IsEnabled = true;
            xyz.IsEnabled = true;

            //Reset Graph labels
            plotter_vertical_title.Content = "";
            plotter_horizontal_title.Content = "";

            Check_For_PlotButton();
        }

        private void Plot_graph_button_Click(object sender, RoutedEventArgs e)
        {
            Clear_button.IsEnabled = true;
            Clear_button.Content = "Clear Graph";
            Plot_graph_button.IsEnabled = false;

            // disable combo boxes and radio buttons 
            sessionGridGestures.IsEnabled = false;
            loadPlotDatabutton2.IsEnabled = false;
            Plot_Type_ComboBox.IsEnabled = false;
            Radio_Grid.IsEnabled = false;
            Body_Segment_ComboBox.IsEnabled = false;
            xyz.IsEnabled = false;

            switch (Plot_Type_ComboBox.SelectedIndex)
            {
                //Initiate the Tracking Plotter
                case 0:
                    plotTracker();
                    break;

                //Initiate the Position Plotter
                case 1:
                    if (Radio_optionA.IsChecked == true)
                    {
                        plotPositionAngle(Body_Segment_ComboBox.SelectedIndex, 0);
                    }
                    else
                    {
                        plotPositionAngle(Body_Segment_ComboBox.SelectedIndex, 1);
                    }
                    break;

                //Initiate the Angular Plotter
                case 2:
                    if (Radio_optionA.IsChecked == true)
                    {
                        plotPositionAngle(Body_Segment_ComboBox.SelectedIndex, 2);
                    }
                    else
                    {
                        plotPositionAngle(Body_Segment_ComboBox.SelectedIndex, 3);
                    }
                    break;
            }
            

        }

        private void plotTracker()
        {
            chart = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            chart2 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            int i = 0;

                        
            
            foreach (PlotGrid pg in sessionGridGestures.ItemsSource)
            {
                if (pg.Selected)
                {
                    double[] firstX = new double[1];
                    double[] firstY = new double[1];


                    //Prepare coordinates for plotting
                    List<float[]> lf_data = sessionGestures.gestures[pg.gkey];
                    List<List<float>> ll_data = toListList(lf_data);

                    String test = ll_data[(Plot_Type_ComboBox.SelectedIndex * 3) + 2].ToString();

                    //Check for the x-y or x-z Radio buttion
                    var x = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 2].AsEnumerable();
                    firstX[0] = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 2][0];

                    var y = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 3].AsEnumerable();
                    firstY[0] = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 3][0];

                    plotter_horizontal_title.Content = "x-axis";

                    if ((bool)Radio_optionB.IsChecked)
                    {
                        y = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 4].AsEnumerable();
                        firstY[0] = ll_data[(Body_Segment_ComboBox.SelectedIndex * 3) + 4][0];
                        plotter_vertical_title.Content = "z-axis";
                    }
                    else
                    {
                        plotter_vertical_title.Content = "y-axis";
                    }

                    float[] xfarray = x.ToArray();
                    float[] yfarray = y.ToArray();

                    double[] xarray = FloatAtoDoubleA(xfarray);
                    double[] yarray = FloatAtoDoubleA(yfarray);

                    var xdata = xarray.AsXDataSource();
                    var ydata = yarray.AsYDataSource();

                   CompositeDataSource   compositeDS = xdata.Join(ydata);

                   Pen pen = new Pen(Brushes.Purple, 3);     
                   CircleElementPointMarker marker = new CircleElementPointMarker
                   {
                       Size = 10,
                       Brush = Brushes.Aqua,
                       Fill = Brushes.Purple
                   }; 

                   switch (i)
                   {
                       case 0:
                         pen =  new Pen(Brushes.Purple, 3);                       
                         break;

                       case 1:
                         pen = new Pen(Brushes.LimeGreen , 3);
                         break;

                       case 2:
                         pen = new Pen(Brushes.SkyBlue, 3);
                         break;

                       default: new Pen(Brushes.MediumPurple, 3);
                         break;
                   }  




                   chart.Add(plotter.AddLineGraph(compositeDS,pen ,marker, new PenDescription(pg.name + " Tracker")));


                    //Adding the starting point to the graph
                   
                    var xDataSource = firstX.AsXDataSource();
                    var yDataSource = firstY.AsYDataSource();
                    compositeDS = xDataSource.Join(yDataSource);

                    chart2.Add(plotter.AddLineGraph(compositeDS, new Pen(Brushes.Orange, 3),
                        new CircleElementPointMarker
                        {
                            Size = 10,
                            Brush = Brushes.Red,
                            Fill = Brushes.Orange
                        },
                        new PenDescription("Starting Point")));

                    plotter.Children.Add(new CursorCoordinateGraph());
                    i++;
                }
            }

            
            plotter.FitToView();
        }

        /*
         * plotGraph sets the data for the chartPlotter inside xaml.
         * @user_axis = sets whether x,y,z coordinates of the joint will be displayed. values are 0,1,2 respectively.
         * @jointnum = sets which joint to be displayed. starts from 0 for hipcenter to 19 for rightfoot
         * @graphtype = sets what information is to be graphed. see the switch statement for its representation.
         * @filepath = csv filepath
         */
        private void plotPositionAngle(int jointnum, int graphtype)
        {
            chart = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            int j = 0;
            
            foreach (PlotGrid pg in sessionGridGestures.ItemsSource)
            {
                if (pg.Selected)
                {

                    int user_axis = -1;
                    if (Radio_X.IsChecked == true)
                    {
                        user_axis = 0;
                    }
                    else if (Radio_Y.IsChecked == true)
                    {
                        user_axis = 1;
                    }
                    else if (Radio_Z.IsChecked == true)
                    {
                        user_axis = 2;
                    }

                    try
                    {
                        if (user_axis == -1)
                        {
                            throw new ArgumentException();
                        }

                        if (jointnum > 19 || jointnum < 0)
                        {
                            throw new ArgumentException();
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine("Invalid arguments in plotGraph()");

                    }

                    //Gestures sessionGestures = readInSessionData(filepath);// will be replaced in future by the selected Gesture in the datagrid
                    List<float[]> lf_data = sessionGestures.gestures[pg.gkey];

                    List<List<float>> ll_data = toListList(lf_data);

                    List<float> x = new List<float>();
                    List<float> y = new List<float>();
                    switch (graphtype)
                    {
                        case 0: //Position
                            for (int i = 0; i < ll_data[0].Count; i++)
                            {
                                x.Add(ll_data[1][i] / 1000);
                            }
                            y = ll_data[jointnum * 3 + user_axis + 2];
                            plotter_horizontal_title.Content = "time(sec)";
                            plotter_vertical_title.Content = "metres";
                            break;

                        case 1: //Position over time
                            for (int i = 1; i < ll_data[0].Count - 1; i++)
                            {
                                x.Add((ll_data[1][i] / 1000));
                                y.Add((ll_data[jointnum * 3 + user_axis + 2][i + 1]
                                    - ll_data[jointnum * 3 + user_axis + 2][i - 1]) / 2);
                                plotter_horizontal_title.Content = "time(sec)";
                                plotter_vertical_title.Content = "metres per sec";
                            }
                            break;
                        case 2: //Angle
                            for (int i = 0; i < ll_data[0].Count; i++)
                            {
                                x.Add(ll_data[1][i] / 1000);
                                y.Add((float)GetBodySegmentAngle(jointnum, ll_data, i));
                            }
                            plotter_horizontal_title.Content = "time(sec)";
                            plotter_vertical_title.Content = "degrees";
                            break;

                        case 3: //Angle over time
                            for (int i = 1; i < ll_data[0].Count - 1; i++)
                            {
                                x.Add((ll_data[1][i] / 1000));
                                y.Add(((float)(GetBodySegmentAngle(jointnum, ll_data, i + 1) - GetBodySegmentAngle(jointnum, ll_data, i - 1))) / 2);
                            }
                            plotter_horizontal_title.Content = "time(sec)";
                            plotter_vertical_title.Content = "degrees per sec";
                            break;

                        default:
                            break;

                    }

                    float[] xfarray = x.ToArray();
                    float[] yfarray = y.ToArray();

                    double[] xarray = FloatAtoDoubleA(xfarray);
                    double[] yarray = FloatAtoDoubleA(yfarray);


                    var xdata = xarray.AsXDataSource();
                    var ydata = yarray.AsYDataSource();

                    CompositeDataSource compositeDS = xdata.Join(ydata);


                    Pen pen = new Pen(Brushes.Purple, 3);
                    CircleElementPointMarker marker = new CircleElementPointMarker
                    {
                        Size = 10,
                        Brush = Brushes.Aqua,
                        Fill = Brushes.Purple
                    };

                    switch (j)
                    {
                        case 0:
                            pen = new Pen(Brushes.Purple, 3);
                            break;

                        case 1:
                            pen = new Pen(Brushes.LimeGreen, 3);
                            break;

                        case 2:
                            pen = new Pen(Brushes.SkyBlue, 3);
                            break;

                        default: new Pen(Brushes.MediumPurple, 3);
                            break;
                    }  


                    chart.Add(plotter.AddLineGraph(compositeDS, pen, marker,
                        new PenDescription(pg.name + " Tracker")));
                        plotter.Children.Add(new CursorCoordinateGraph());
                        j++;

                }
            }
            plotter.FitToView();


        }

        private void Check_For_PlotButton()
        {

            bool selected = false;
            foreach (PlotGrid pg in sessionGridGestures.ItemsSource)
            {
                if (pg.Selected)
                {
                    selected = true;
                    break;
                }
            }

            if ((isFileloaded && isRowSelected) && isPlotterSelected
                && isBodySegmentSelected && !Clear_button.IsEnabled && selected)
            {
                Plot_graph_button.IsEnabled = true;
            }
            else
            {
                Plot_graph_button.IsEnabled = false;
            }
        }

        public static double[] FloatAtoDoubleA(float[] input)
        {
            if (input == null)
            {
                return null; // Or throw an exception - your choice
            }
            
            double[] output = new double[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = input[i];
            }

            return output;
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
                    char ch = firstline[3].ElementAt(firstline[3].Length - 1);
                    int rating = (ch.CompareTo('A'));
                    
                    if (rating < 0 || rating > 4)
                    {
                        rating = 5;                    
                    }
                    
                    _rating = (GestureKey.Rating)(rating);
                    if (firstline[3].EndsWith("DEFAULT"))
                    {
                        firstline[3] = firstline[3].Remove(firstline[3].Length - 8);
                    }
                    else
                    {
                        char[] trim = { 'A', 'B', 'C', 'D', 'E' };
                        firstline[3] = firstline[3].TrimEnd(trim);
                    }
                    _name = firstline[3];
                }
                _framenum = (int)(float_data[float_data.Count - 1][0]);
                _timestamp = new TimeSpan((long)(float_data[float_data.Count - 1][1] * 10000));

                GestureKey dataKey = new GestureKey(_rating, _name, _recorded, _framenum, _timestamp);

                sessionGestures.gestures.Add(dataKey, float_data);
            }

           
            sessionGestures.gestures.Remove(sessionGestures.gestures.Keys.ElementAt(0));
            return sessionGestures;
        }

        private void writeToFile(string filelocation, Gestures session)
        {
            File.Delete(filelocation);
            System.IO.StreamWriter write_file = new System.IO.StreamWriter(filelocation, true);

            string str = "Frame Number, Time Stamp,";
            Skeleton skeletonText = new Skeleton();
            foreach (Joint joint in skeletonText.Joints)
            {
                str = str + joint.JointType.ToString() + " X" + ","
                          + joint.JointType.ToString() + " Y" + ","
                          + joint.JointType.ToString() + " Z" + ",";
            }

            foreach (KeyValuePair<GestureKey, List<float[]>> session_kvp in session.gestures)
            {
                string str1 = "Start:, " + session_kvp.Key.recorded.ToString() + "," + "Gestures:, " + session_kvp.Key.name + session_kvp.Key.rating.ToString();
                write_file.WriteLine(str1);
                write_file.WriteLine(str);

                List<string[]> str_Value = session_kvp.Value.ConvertAll( new Converter<float[], string[]>(FloatAtoStringA));

                foreach (string[] line in str_Value)
                {
                    string str_line = string.Join(",", line);
                    write_file.WriteLine(str_line);
                }

                DateTime stop = session_kvp.Key.recorded.Add(session_kvp.Key.timestamp);
                string str2 = "Stop:, " + stop.ToString();
                write_file.WriteLine(str2);
            }

            write_file.Close();
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

        public static string[] FloatAtoStringA(float[] floatA)
        {
            string[] strA = new string[floatA.Length];
            for (int i = 0; i < floatA.Length - 1; i++)
            {
                strA[i] = floatA[i].ToString();
            }

            return strA;
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

        public List<List<float>> toListList(List<float[]> listArray)
        {
            List<List<float>> listList = new List<List<float>>(62);

            for (int j = 0; j < 62; j++)
            {
                List<float> list = new List<float>(listArray.Count);

                for (int i = 0; i < listArray.Count; i++)
                {
                    list.Add(listArray[i][j]);
                }

                listList.Add(list);
            }
            return listList;
        }

        public double GetBodySegmentAngle(int joint_center, List<List<float>> data, int frame)
        {
            int joint_parent = -1;
            int joint_child = -1;
            switch (joint_center)
            {
                case 1:
                    joint_parent = 0;
                    joint_child = 2;
                    break;

                case 4:
                case 8:
                    joint_parent = 2;
                    joint_child = joint_center + 1;
                    break;

                case 12:
                case 16:
                    joint_parent = 0;
                    joint_child = joint_center + 1;
                    break;

                case 5:
                case 6:
                case 9:
                case 10:
                case 13:
                case 14:
                case 17:
                case 18:
                    joint_parent = joint_center - 1;
                    joint_child = joint_center + 1;
                    break;

                default:
                    //error
                    break;
            }


            //joints are stored in sets of 3(x,y,z coordinates)
            //offset of 2 for frame number and timestamp
            Vector3 vectorJoint1ToJoint2 = new Vector3(data[joint_parent * 3 + 2][frame] - data[joint_center * 3 + 2][frame], data[joint_parent * 3 + 3][frame] - data[joint_center * 3 + 3][frame], data[joint_parent * 3 + 4][frame] - data[joint_center * 3 + 4][frame]);
            Vector3 vectorJoint2ToJoint3 = new Vector3(data[joint_center * 3 + 2][frame] - data[joint_child * 3 + 2][frame], data[joint_center * 3 + 3][frame] - data[joint_child * 3 + 3][frame], data[joint_center * 3 + 4][frame] - data[joint_child * 3 + 4][frame]);
            vectorJoint1ToJoint2.Normalize();
            vectorJoint2ToJoint3.Normalize();

            Vector3 crossProduct = Vector3.Cross(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double crossProductLength = crossProduct.Z;
            double dotProduct = Vector3.Dot(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double segmentAngle = Math.Atan2(crossProductLength, dotProduct);

            // Convert the result to degrees.
            double degrees = segmentAngle * (180 / Math.PI);

            return degrees;


        }

        private void sessionGridGestures_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Check_For_PlotButton();
        }

        

    }
}
