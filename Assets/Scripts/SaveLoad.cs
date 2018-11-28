using UnityEngine;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System;
using System.IO;

/*
 * Setting file formation
 * Each option in a new line
 * Audio feedback enabled : 0 , 1
 * Graphic feedback enabled : 0, 1
 * Clicker enabled : 0,1
 */
//parse each line
//string[] codes  = string.Split(',');

public class SaveLoad : MonoBehaviour
{
    //private string dir;
    private string settingsPath;

    private void Start()
    {
        //dir = Application.persistentDataPath + "/../";
        //settingsPath = dir + "settings.txt";
        settingsPath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (!File.Exists(settingsPath)) //First run setting.txt doesn't exist
            CreateSettings(new List<int> { 1, 1, 1 });
    }
    public List<int> LoadSettings()
    {
        List<int> settings = new List<int>();
        try
        {
            using (StreamReader sr = new StreamReader(new FileStream(settingsPath, FileMode.Open)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    settings.Add((int)Char.GetNumericValue(line.ToCharArray()[0]));
                }
            }
        }
        catch (Exception e)
        {
            settings.Add(1); // Default value
            Debug.LogError("Failed to read line and  " + e.Message);
        };
        return settings;
    }

    public void SaveSettings(List<int> settings)
    {
        /*
        StreamWriter writer = new StreamWriter(new FileStream(settingsPath, FileMode.OpenOrCreate)); //Overwrite file
        foreach (int i in  settings)
        {
            writer.WriteLine(i.ToString());
        }
        writer.Dispose();
        */
        using (TextWriter writer = File.CreateText(settingsPath))
        {
            foreach (int i in settings)
            {
                writer.WriteLine(i.ToString());
            }
        }
    }

    private void CreateSettings(List<int> settings)
    {
        /*
            StreamWriter writer = new StreamWriter(new FileStream(settingsPath, FileMode.CreateNew)); //Overwrite file
            foreach (int i in settings)
            {
                writer.WriteLine(i.ToString());
            }
            writer.Dispose();
        */
        using (TextWriter writer = File.CreateText(settingsPath))
        {
            foreach (int i in settings)
            {
                writer.WriteLine(i.ToString());
            }
        }
    }
}