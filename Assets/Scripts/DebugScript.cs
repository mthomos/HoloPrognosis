using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public GameObject obj1, obj2;
    public UtilitiesScript utilities;
    public bool res;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello World");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vec_from = new Vector2(obj1.transform.position.x, obj1.transform.position.z);
        Vector2 vec_to = new Vector2(obj2.transform.position.x, obj2.transform.position.z);
        Debug.Log(Vector2.SignedAngle(vec_from, vec_to));
    }
}
