using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SaveFile
{
}

public class SaveData
{
    const string SaveFileName = "savedata_{0}.json";
    public static int SaveFileId = 0;

    public SaveData()
    {
        // Default save file here.
        this.GameVersion = ProjectSettings.GetSetting("application/config/version", "0.0.0").AsString();

    }

    [JsonInclude] public string GameVersion { get; set; }    // Important if you break compatibility between saves. That will let you make migrations.
    public class LevelSave
    {
        [JsonInclude] public bool Unlocked;
        [JsonInclude] public System.Collections.Generic.Dictionary<string, int> Items = new System.Collections.Generic.Dictionary<string, int>();  // Red rings and stuff
        [JsonInclude] public System.Collections.Generic.Dictionary<string, double> BestTime = new System.Collections.Generic.Dictionary<string, double>();  // Key: character name, Value: best time in seconds // TODO: Ranking?
        [JsonInclude] public System.Collections.Generic.Dictionary<string, string> BestRank = new System.Collections.Generic.Dictionary<string, string>();  // Key: character name, Value: best rank
    }
    [JsonInclude] public System.Collections.Generic.Dictionary<string, LevelSave> LevelSaves = new System.Collections.Generic.Dictionary<string, LevelSave>();
    [JsonInclude] public System.Collections.Generic.Dictionary<string, int> GlobalItems = new System.Collections.Generic.Dictionary<string, int>();

    public static SaveData Load()
    {
        try { 
            if (!FileAccess.FileExists("user://" + string.Format(SaveFileName, SaveFileId)))
            {
                return new SaveData(); // No save data
            }
            using var saveFile = FileAccess.Open("user://" + string.Format(SaveFileName, SaveFileId), FileAccess.ModeFlags.Read);
            var serializedSave = saveFile.GetAsText();
            var loadedSave = JsonSerializer.Deserialize<SaveData>(serializedSave);
            return loadedSave;
        } catch (Exception e)
        {
            ErrorLog.AddMessage(e.Message + e.StackTrace.ToString());
            return new SaveData();
        }
    }

    public void Save()
    {
        try
        {
            this.GameVersion = ProjectSettings.GetSetting("application/config/version", "0.0.0").AsString();

            var serializedSave = JsonSerializer.Serialize(this);
            using var saveFile = FileAccess.Open("user://" + string.Format(SaveFileName, SaveFileId), FileAccess.ModeFlags.Write);
            saveFile.StoreString(serializedSave);
        } catch (Exception e)
        {
            ErrorLog.AddMessage(e.Message + e.StackTrace.ToString());
        }
    }

    public static void Erase()
    {
        new SaveData().Save();
    }

    public void UnlockLevel(string stageIdentifier)
    {
        var stageSave = this.LevelSaves.GetValueOrDefault(stageIdentifier, new LevelSave());
        stageSave.Unlocked = true;
        this.LevelSaves[stageIdentifier] = stageSave;
    }

    public bool IsLevelUnlocked(string stageIdentifier)
    {
        var stageSave = this.LevelSaves.GetValueOrDefault(stageIdentifier, new LevelSave());
        return stageSave.Unlocked;
    }
}
