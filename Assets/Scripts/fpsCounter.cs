using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    public float updateInterval = 0.1F;
    private double lastInterval;
    private int frames = 0;
    private float fps;
    private TextMesh text;
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
        text = this.GetComponent<TextMesh>();
    }

    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            text.text = fps + "fps";
            frames = 0;
            lastInterval = timeNow;
        }
    }
}