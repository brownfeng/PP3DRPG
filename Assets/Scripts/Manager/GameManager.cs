using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    private CinemachineFreeLook followCamera;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void RegisterPlayer(CharacterStats player)
    {
        playerStats = player;
        // 需要找到当前场景的 FreeLook Camera
        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if (followCamera != null)
        {
            followCamera.LookAt = player.transform.GetChild(2);
            followCamera.Follow = player.transform.GetChild(2);
        }
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (IEndGameObserver observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}
