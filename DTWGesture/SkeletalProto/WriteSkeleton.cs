using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Kinect;

public class WriteSkeleton
{
    

	public void WriteSkeletonToFile(Skeleton skeleton, SkeletonFrame skeletonFrame, System.IO.StreamWriter file)
	{
        string frame = Convert.ToString(skeletonFrame.FrameNumber);
        string time = Convert.ToString(skeletonFrame.Timestamp);
        string str = frame + " , " + time;

        foreach (Joint joint in skeleton.Joints)
        {
             str = str + " , "
                + joint.Position.X.ToString() + " , "
                + joint.Position.Y.ToString() + " , "
                + joint.Position.Z.ToString();

        }

        file.WriteLine(str);
	}
}
