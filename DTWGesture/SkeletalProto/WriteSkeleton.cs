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
        int n = 0;
        JointCollection joints = skeleton.Joints;
        string frame = Convert.ToString(skeletonFrame.FrameNumber);
        string time = Convert.ToString(skeletonFrame.Timestamp);
        System.Text.StringBuilder sb = new System.Text.StringBuilder(150);
        sb.Append(frame);
        sb.Append(" , ");
        sb.Append(time);
        sb.Append(" , ");
        float[] positions = new float[60];

        foreach (Joint joint in joints)
        {
            positions[n] = joint.Position.X;
            positions[n + 1] = joint.Position.Y;
            positions[n + 2] = joint.Position.Z;
            n += 3;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            sb.Append(positions[i]);
            sb.Append(',');
        }
        string str = sb.ToString();
        file.WriteLine(str);
	}
}
