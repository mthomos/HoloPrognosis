using UnityEngine;

public class WindEffect : MonoBehaviour
{
    public Vector3 windDirection;
    public float windSpeed;
    public float windUpdate;
    public float updateScale;
    private float wind;
    private float direction;
    // Use this for initialization
    void Start()
    {
        updateScale = 1.0f;
        windDirection = Vector3.zero;
        windSpeed = .0f;
        if (windUpdate == .0f) windUpdate = 2.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float interval = Time.deltaTime;
        if (Time.time % windUpdate == 0.0f)
        {
            wind = Random.Range(0, 3);
            direction = Random.Range(0, 3);
            windSpeed = wind;
            windDirection.x = direction;
            windDirection.y = direction;
            windDirection.z = direction;
            updateScale = 1.0f;
        }
        else
        {
            updateScale = updateScale - interval / windUpdate;
            windSpeed = updateScale * wind;
            windDirection.x = updateScale * direction;
            windDirection.y = updateScale * direction;
            windDirection.z = updateScale * direction;
        }


    }
}