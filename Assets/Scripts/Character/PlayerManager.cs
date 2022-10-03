using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerManager : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //MouseManager.Instance.OnMouseClick += OnMouseClick; // 因为这里的 MouseManager.Instance 可能为空

    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClick += OnMouseClick;
    }

    private void OnMouseClick(Vector3 vector) {
        agent.destination = vector;
    }

}
