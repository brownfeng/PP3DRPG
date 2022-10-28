using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    // 初始值可以随便写, 只是作为键值对的关键帧
    string sceneName = "level";

    public string SceneName { get { return PlayerPrefs.GetString(sceneName); } }


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        // 为了在游戏场景中, 需要能添加游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }

    public void Save(Object data, string key)
    {
        var jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(Object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            var json = PlayerPrefs.GetString(key);
            JsonUtility.FromJsonOverwrite(json, data);
        }
    }
}
