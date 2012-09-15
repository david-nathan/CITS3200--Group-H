using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace SkeletalProto
{
   class SkeletonAngles
    {
        private int _RotationOffset = 0;
        private bool _ReverseCoordinates = false;
        private JointType _JointType1;
        private JointType _JointType2;
        private JointType _JointType3;

        public int RotationOffset
        {
            get { return _RotationOffset; }
            set
            {
                // Use the modulo in case the rotation value specified exceeds
                // 360.
                _RotationOffset = value % 360;
            }
        }

        public bool ReverseCoordinates
        {
            get { return _ReverseCoordinates; }
            set { _ReverseCoordinates = value; }
        }

        public void SetBodySegments(JointType JointType1, JointType JointType2, JointType JointType3)
        {
            _JointType1 = JointType1;
            _JointType2 = JointType2;
            _JointType3 = JointType3;
        }

        public double CalculateReverseCoordinates(double degrees)
        {
            return (-degrees + 180) % 360;
        }

        /// <summary>
        /// Calculates the angle between the segments of the body defined by the specified joints.
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="joint1"></param>
        /// <param name="joint2">Must be between joint1 and joint3</param>
        /// <param name="joint3"></param>
        /// <returns>The angle in degrees between the specified body segmeents.</returns>
        public double GetBodySegmentAngle(Skeleton skeleton)
        {

            JointCollection joints = skeleton.Joints;

            Joint joint1 = joints[_JointType1];
            Joint joint2 = joints[_JointType2];
            Joint joint3 = joints[_JointType3];

            Vector3 vectorJoint1ToJoint2 = new Vector3(joint1.Position.X - joint2.Position.X, joint1.Position.Y - joint2.Position.Y, 0);
            Vector3 vectorJoint2ToJoint3 = new Vector3(joint2.Position.X - joint3.Position.X, joint2.Position.Y - joint3.Position.Y, 0);
            vectorJoint1ToJoint2.Normalize();
            vectorJoint2ToJoint3.Normalize();

            Vector3 crossProduct = Vector3.Cross(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double crossProductLength = crossProduct.Z;
            double dotProduct = Vector3.Dot(vectorJoint1ToJoint2, vectorJoint2ToJoint3);
            double segmentAngle = Math.Atan2(crossProductLength, dotProduct);

            // Convert the result to degrees.
            double degrees = segmentAngle * (180 / Math.PI);

            // Add the angular offset.  Use modulo 360 to convert the value calculated above to a range
            // from 0 to 360.
            degrees = (degrees + _RotationOffset) % 360;

            // Calculate whether the coordinates should be reversed to account for different sides 
            if (_ReverseCoordinates)
            {
                degrees = CalculateReverseCoordinates(degrees);
            }

            return degrees;
        }
    }
}

  
