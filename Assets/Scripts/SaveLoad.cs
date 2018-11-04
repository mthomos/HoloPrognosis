using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class SaveLoad
{
    public static List<SessionData> savedGames = new List<SessionData>();

    public static void Save(SessionData data)
    {
        SaveLoad.savedGames.Add(data);
        BinaryFormatter bf = new BinaryFormatter();
        var fileName = "/session_" + data.date.ToString("d") + "_" + data.date.Hour.ToString() +":" + data.date.Minute.ToString() + ".sd";
        FileStream file = File.Create(Application.persistentDataPath + fileName ); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }

    public static void Load()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "session_*.*");
        foreach (string name in files)
        {
            if (File.Exists(Application.persistentDataPath + "/"+ name))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + name, FileMode.Open);
                SaveLoad.savedGames.Add((SessionData)bf.Deserialize(file));
                file.Close();
            }
        }
    }
}