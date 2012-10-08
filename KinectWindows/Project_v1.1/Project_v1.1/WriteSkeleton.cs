using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Kinect;

public class WriteSkeleton
{
    public float[] WriteSkeletonToFile(Skeleton skeleton, SkeletonFrame skeletonFrame, System.IO.StreamWriter file)
    {

        int count = 2;
        float[] dataLine = new float[63];
        System.Text.StringBuilder sb = new System.Text.StringBuilder(150);
        string str_framenum = skeletonFrame.FrameNumber.ToString();
        dataLine[0] = float.Parse(str_framenum);
        sb.Append(str_framenum);
        sb.Append(" , ");
        string str_timestamp = skeletonFrame.Timestamp.ToString();
        dataLine[1] = float.Parse(str_timestamp);
        sb.Append(str_timestamp);
        sb.Append(" , ");

        foreach (Joint joint in skeleton.Joints)
        {

            dataLine[count] = joint.Position.X;
            sb.Append(joint.Position.X.ToString());
            sb.Append(" , ");
            dataLine[count + 1] = joint.Position.Y;
            sb.Append(joint.Position.Y.ToString());
            sb.Append(" , ");
            dataLine[count + 2] = joint.Position.Z;
            sb.Append(joint.Position.Z.ToString());
            sb.Append(" , ");
            count = count + 3;
        }

        string str = sb.ToString();

        file.WriteLine(str);
        return dataLine;
    }
}
