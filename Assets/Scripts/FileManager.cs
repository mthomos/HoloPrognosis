using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_WSA && !UNITY_EDITOR
using Windows.Storage;
using System.Threading.Tasks;
#endif

public struct FileStruct
{
    public string fileName;
    public string content;

    public FileStruct (string fileName, string content)
    {
        this.fileName = fileName;
        this.content = content;
    }
}

public class FileManager : MonoBehaviour
{
    private static string settingsName = "settings.txt";

    private static string appPath;
    private static string output;
    private static Queue<FileStruct> writeBuffer = new Queue<FileStruct>();

    public static FileManager Instance { get; private set; }

    private void Start()
    {
        if(appPath == null)
            appPath = Application.persistentDataPath;

        Instance = this;
    }

    private void Update()
    {
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if (writeBuffer.Count > 0)
        {
            Debug.Log("Try to write");
            FileStruct current = writeBuffer.Dequeue();
            Task task = WriteFile(current);
            task.Wait();
        }
    }

    private static async Task WriteFile(FileStruct current)
    {
#if UNITY_WSA && !UNITY_EDITOR
        StorageFolder folder = ApplicationData.Current.LocalFolder;
        StorageFile file;
        if (File.Exists(Path.Combine(appPath, current.fileName)))
        {
            file = await folder.GetFileAsync(current.fileName);
            Debug.Log("File exist");
        }
        else
        {
            file = await folder.CreateFileAsync(current.fileName);
            Debug.Log("File doesnt exist");
        }
        try
        {
            Debug.Log("Trying to write");
            await FileIO.WriteTextAsync(file, current.content);
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing in file->" + current.fileName + "_________" + e);
            //Insert to buffer again
            writeBuffer.Enqueue(current);
        }
#endif
    }

    public void addString(string file, string content)
    {
        writeBuffer.Enqueue(new FileStruct(file, content));
    }

    public string readFile(string fileName)
    {
        output=null;

        if (!File.Exists(Path.Combine(appPath, fileName)))
            return "Not_found";

        Task task = ReadFile(fileName);
        task.Wait();
        return output;
    }

    private static async Task ReadFile(string fileName)
    {
#if UNITY_WSA && !UNITY_EDITOR
        StorageFolder folder = ApplicationData.Current.LocalFolder;
        StorageFile file = await folder.GetFileAsync(fileName);
        try
        {
            output = await FileIO.ReadTextAsync(file);
        }
        catch (Exception e)
        {
            Debug.Log("Error reading  file->" + fileName + "___" + e);
        }
#endif
    }
}
