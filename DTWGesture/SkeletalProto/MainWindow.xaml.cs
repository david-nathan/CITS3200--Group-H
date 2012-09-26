﻿
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using System.IO;
using System;
using System.Collections.Generic;
using System.Timers;


namespace SkeletalProto
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private SkeletonAngles angleCalculation = new SkeletonAngles();
        private Boolean recording;
        private Boolean firstframe = true;
        private int initFrameNum;
        private long initTimeStamp;
        private List<float[]> recordData = new List<float[]>();
        private List<float[]> gestureData;
       
        

        System.IO.StreamWriter file;
        
        public MainWindow()
        {
            InitializeComponent();
            recording = false;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
        }
        
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusTextBlock.Text = Properties.Resources.NoKinectReady;
                button1.IsEnabled = false;
            }
                
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }

        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            SkeletonFrame skelFrame = e.OpenSkeletonFrame();
            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null )
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                   
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                            angleCalculation.SetBodySegments(JointType.WristRight, JointType.ElbowRight, JointType.ShoulderRight);
                            double angle = angleCalculation.GetBodySegmentAngle(skel);
                            string angleString = Convert.ToString(angle);
                            this.angleTextBlock.Text = angleString;

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
                                WriteSkeleton writeskel = new WriteSkeleton();
                                float[] data = writeskel.WriteSkeletonToFile(skel, skelFrame, file);
                                recordData.Add(data);
                            }
                            
                            
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }

            if (skelFrame != null)
            {
                skelFrame.Dispose();
            }
        }

        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            recording = true;
            firstframe = true;
            button2.IsEnabled = true;
            button1.IsEnabled = false;
            string date = DateTime.Now.ToString();
            string str = "Start:, " + date + " , ";
            Skeleton skeleton = new Skeleton();
            foreach (Joint joint in skeleton.Joints)
            {
                str = str + joint.JointType.ToString() + " , , , ";
            }
            file = new System.IO.StreamWriter(@"C:\Users\Public\WriteText.csv", true);
            file.WriteLine(str);
            
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            
            string date = DateTime.Now.ToString();
            string str = "Stop:, " + date;
            file.WriteLine(str);
            file.Close();
            recording = false;
            button1.IsEnabled = true;
            button2.IsEnabled = false;
            button3.IsEnabled = true;
           
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

            readInText();
            gestureData = standardiseData(gestureData);
            recordData = standardiseData(recordData);
            float[] gestureVariance = variance(gestureData);
            float[] recordVariance = variance(recordData);
            string rdCount = recordData.Count.ToString();
            string gdCount = gestureData.Count.ToString();
            string counts = "COUNT:";
            counts += rdCount + gdCount;  //For DEBUGGING
            Console.WriteLine(counts);
        }



        private void readInText()
        {
            List<string[]> str_gestureData = parseCSV(@"C:\Users\Public\JumpText.csv");
            str_gestureData.RemoveAt(0); //Remove start date
            str_gestureData.RemoveAt(str_gestureData.Count-1); //Remove stop date
               
                gestureData = str_gestureData.ConvertAll(
                    new Converter<string[], float[]>(StringAtoFloatA)); //Convert to float
            button3.IsEnabled = false;
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
                MessageBox.Show(e.Message);
            }

            return parsedData;
        }


        private float[] variance(List<float[]> dataList)
        {
            float[] var = new float[62];   //array of variances
            float[] avg = new float[62];   //array of averages

            for (int j = 2; j < 62; j++) //average calculation
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    avg[j] += dataList[i][j];

                }

                avg[j] = avg[j] / (dataList.Count);

            }

            for (int j = 2; j < 62; j++) //variance calculation
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var[j] = var[j] + (dataList[i][j] - avg[j]) * (dataList[i][j] - avg[j]);

                }

                var[j] = var[j] / (dataList.Count);
            }

            return var;
        }



        private List<float[]> standardiseData(List<float[]> dataList)
        {
            float[] avg = new float[62];   //array of averages
            float[] stdev = new float[62]; //array of standard deviations

            for (int j = 2; j < 62; j++) //average calculation
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    avg[j] += dataList[i][j];

                }

                avg[j] = avg[j] / (dataList.Count);

            }

            for (int j = 2; j < 62; j++) //standard deviation calculation
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    stdev[j] = stdev[j] + (dataList[i][j] - avg[j]) * (dataList[i][j] - avg[j]);

                }

                stdev[j] = stdev[j] / (dataList.Count);
                stdev[j] = (float)Math.Sqrt(stdev[j]);

            }

            for (int j = 2; j < 62; j++) //transform dataList to standardised data
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i][j] = (dataList[i][j] - avg[j]) / stdev[j];
                }
            }

            return dataList;
        }

       





   
    }
}
