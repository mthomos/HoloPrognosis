using System.IO;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public UtilitiesScript utilities;
    public FileManager fileManager;
    private Stream debugStream;
    private int triggered;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Camera.main.transform.forward);
        Debug.Log("_____");
        Debug.Log(Camera.main.transform.eulerAngles);
        //
        if (fileManager == null)
            fileManager = new FileManager();

        debugStream = fileManager.OpenFileForWriteAsync("debug.txt");
    }

    // Update is called once per frame
    void Update()
    {
       if (triggered <10)
       {
            /*
            Debug.Log("write ___" + triggered);
            string newPath = Path.Combine(fileManager.getAppPath(), "test.txt");
            var bytes = System.Text.Encoding.UTF8.GetBytes("test" + triggered);
            File.WriteAllBytes(newPath, bytes);
            */
            fileManager.addRequest("test.txt", "test" + triggered);
            triggered++;
       }
    }
}
