using System;
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
        private int initFrameNum;
        private long initTimeStamp;
        private string SaveFileLocation = @"C:\Users\Public";
        

      




        public MainWindow()
        {
            InitializeComponent();
            recording = false;
            stop_button.IsEnabled = false;
            start_button.IsEnabled = false;
            classify_button.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
          
        }

        

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;

            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;

            if (newSensor == null)
            {
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
        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
            recording = true;
            start_button.IsEnabled = false;
            stop_button.IsEnabled = true;
            TargetFileButton.IsEnabled = false;
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
            stop_button.IsEnabled = false;
            start_button.IsEnabled = true;
            string date = DateTime.Now.ToString();
            string str = "Stop:, " + date;
            file.WriteLine(str);
            file.Close();
            recording = false;
            classify_button.IsEnabled = true;
            TargetFileButton.IsEnabled = true;
        }

        private void targetFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            SaveFileDialog dlg = new SaveFileDialog();

            // Set filter for file extension and default file extension
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
                    classify_button.IsEnabled = true;
                    start_button.IsEnabled = true;
                }
            }


        }

        private void classify_button_Click(object sender, RoutedEventArgs e)
        {
            TargetFileButton.IsEnabled = false;
            start_button.IsEnabled = false;




        }



    }
}
