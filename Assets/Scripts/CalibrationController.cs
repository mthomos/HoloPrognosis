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

    public void AddValue(float headHandDist, float handHeight)
    {
        if (rightPoseInProgess)
        {
            Right_Pose_headHandDistance = headHandDist;
            Right_Pose_handHeight = handHeight;
        }
        else
        {
            Highest_Pose_headHandDistance = headHandDist;
            Highest_pose_handHeight = handHeight;
        }
    }

    public float GetRightPoseHeadHandDistance()
    {
        return Right_Pose_headHandDistance;
    }

    public float GetRightPoseHandHeight()
    {
        return Right_Pose_handHeight;
    }

    public float GetHighestPoseHeadHandDistance()
    {
        return Highest_Pose_headHandDistance;
    }

    public float GetHighestPoseHandHeight()
    {
        if (Highest_pose_handHeight> Right_Pose_handHeight )
            return Highest_pose_handHeight;
        else
            return Right_Pose_handHeight;
    }

    public bool IsRightHand()
    {
        return rightHand;
    }

    public void FinishRightPose()
    {
        rightPoseInProgess = false;
    }
}
