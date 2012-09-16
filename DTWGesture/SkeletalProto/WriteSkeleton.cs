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

            System.Text.StringBuilder sb = new System.Text.StringBuilder(150);
            sb.Append(skeletonFrame.FrameNumber.ToString());
            sb.Append(" , ");
            sb.Append(skeletonFrame.Timestamp.ToString());
            sb.Append(" , ");

            foreach (Joint joint in skeleton.Joints){
            
                sb.Append(joint.Position.X.ToString());
                sb.Append(" , ");
                sb.Append(joint.Position.Y.ToString());
                sb.Append(" , ");
                sb.Append(joint.Position.Z.ToString());
                sb.Append(" , ");
            }

            string str = sb.ToString();
            file.WriteLine(str);        
	}
}
