public class CalibrationController
{
    //Right Pose Variables
    private float Right_Pose_headHandDistance;
    private float Right_Pose_handHeight;
    //Highest Position Variables
    private float Highest_Pose_headHandDistance;
    private float Highest_pose_handHeight;
    //Control Variables
    private bool rightHand;
    private bool rightPoseInProgess;

    public CalibrationController(bool hand)
    {
        rightHand = hand;
        rightPoseInProgess = true;
        Right_Pose_headHandDistance = -1;
        Right_Pose_headHandDistance = -1;
        Highest_Pose_headHandDistance = 1;
        Highest_pose_handHeight = -1;
    }

    public void addValue(float headHandDist, float handHeight)
    {
        if (rightPoseInProgess)
        {
            if (headHandDist > Right_Pose_headHandDistance)
                Right_Pose_headHandDistance = headHandDist;
            if (handHeight > Right_Pose_handHeight)
                Right_Pose_handHeight = handHeight;
        }
        else
        {
            if (headHandDist > Highest_Pose_headHandDistance)
                Highest_Pose_headHandDistance = headHandDist;
            if (handHeight > Highest_pose_handHeight)
                Highest_pose_handHeight = handHeight;
        }
    }

    public float getRightPoseHeadHandDistance()
    {
        return Right_Pose_headHandDistance;
    }

    public float getRightPoseHandHeight()
    {
        return Right_Pose_handHeight;
    }

    public float getHighestPoseHeadHandDistance()
    {
        return Highest_Pose_headHandDistance;
    }

    public float getHighestPoseHandHeight()
    {
        return Highest_pose_handHeight;
    }

    public bool isRightHand()
    {
        return rightHand;
    }

    public void finishRightPose()
    {
        rightPoseInProgess = false;
    }
}
