using System.IO;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public UtilitiesScript utilities;
    public FileManager fileManager;
    private Stream debugStream;
    public GameObject obj;
    private int triggered;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(obj.transform.forward);
        Debug.Log("_____");
        //Debug.Log(obj.transform.eulerAngles);
        //
        if (fileManager == null)
            fileManager = new FileManager();

        debugStream = fileManager.OpenFileForWriteAsync("debug.txt");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
