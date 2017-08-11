// WaveGestureSegments.cs
using Microsoft.Kinect;

    public interface IGestureSegment
    {
        GesturePartResult Update(Body skeleton);
    }

    public class WaveSegment1 : IGestureSegment
    {
        public GesturePartResult Update(Body skeleton)
        {
            // Hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y >
                skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X >
                    skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return GesturePartResult.Succeeded;
                }
            }

            // Hand dropped
            return GesturePartResult.Failed;
        }
    }

    public class WaveSegment2 : IGestureSegment
    {
        public GesturePartResult Update(Body skeleton)
        {
            // Hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y >
                skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hand left of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X <
                    skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return GesturePartResult.Succeeded;
                }
            }

            // Hand dropped
            return GesturePartResult.Failed;
        }
    }


// GesturePartResult.cs
    public enum GesturePartResult
    {
        Failed,
        Succeeded
    }