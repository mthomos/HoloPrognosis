using System.IO;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public FileManager fileManager;
    public TextToSpeech textToSpeech;
    private bool trigerred1, trigerred2;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {

        textToSpeech.StartSpeaking("Welcome to the Holographic App ! You can use Gaze, Gesture and Voice Command to interact with it!");
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void debugStartIO()
    {
        timer = Time.time;
        string settingsPath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (File.Exists(settingsPath)) //First run setting.txt doesn't exist
        {
            File.Delete(settingsPath);
        }
    }

    private void debugUpdateIO()
    {
        if (Time.time - timer > 2 && !trigerred1)
        {
            fileManager.addString("settings.txt", "1 \n0 \n0 \n1 ");
            trigerred1 = true;
        }

        if (Time.time - timer > 5 && !trigerred2)
        {
            string text = fileManager.readFile("settings.txt");
            Debug.Log("TEXT------------" + text);
            trigerred2 = true;
        }
    }
}
