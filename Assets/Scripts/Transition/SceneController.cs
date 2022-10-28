using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>
{
    private GameObject player;
    private NavMeshAgent playerAgent;

    public GameObject playerPrefab;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                /// 开始协程
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }

    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        // TODO: 传送的时候, 保存数据
        SaveManager.Instance.SavePlayerData();
        // TODO: 
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            // 异步加载场景
            yield return SceneManager.LoadSceneAsync(sceneName);

            // 异步生成角色
            var transform = GetDestination(destinationTag).transform;
            yield return Instantiate(playerPrefab, transform.position, transform.rotation);
            SaveManager.Instance.LoadPlayerData();

            // 异步读取数据
            yield break;
        }
        else
        {
            // 相同场景
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            var transform = GetDestination(destinationTag).transform;
            player.transform.SetPositionAndRotation(transform.position, transform.rotation);
            playerAgent.enabled = true;
            yield return null;
        }

    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();
        foreach (var entrance in entrances)
        {
            if (entrance.destinationTag == destinationTag)
            {
                return entrance;
            }
        }
        return null;
    }

    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    // 新的场景. 如果要新的场景, 传入新的场景名称
    IEnumerator LoadLevel(string scene)
    {
        if (scene != "")
        {
            yield return SceneManager.LoadSceneAsync(scene);
            // 此时当前的 Scene 已经完全加载

            // 找到一个 spawn point

            var transform = GameManager.Instance.GetEntrance();
            yield return player = Instantiate(playerPrefab, transform.position, transform.rotation
                );

            // 保存游戏
            SaveManager.Instance.SavePlayerData();
            yield break;
        }
    }

    IEnumerator LoadMain()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield break;
    }
}
