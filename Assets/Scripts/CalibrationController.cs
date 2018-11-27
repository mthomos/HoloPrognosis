public class CalibrationController
{
    private bool rightHand;
    private float headHandDistance;
    private float handHeight;

    public CalibrationController(bool hand)
    {
        rightHand = hand;
        headHandDistance = -1;
        handHeight = -1;
    }

    public void addValue(float headHand, float hand)
    {
        if (headHand > headHandDistance)
            headHandDistance = headHand;
        if (hand > handHeight)
            handHeight = hand;
    }

    public float getDistance()
    {
        return headHandDistance;
    }

    public float getHandHeight()
    {
        return handHeight;
    }

    public bool isRightHand()
    {
        return rightHand;
    }
}
