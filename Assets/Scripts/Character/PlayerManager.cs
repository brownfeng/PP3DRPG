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
        //MouseManager.Instance.OnMouseClick += OnMouseClick; // ��Ϊ����� MouseManager.Instance ����Ϊ��

    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClick += OnMouseClick;
    }

    private void OnMouseClick(Vector3 vector) {
        agent.destination = vector;
    }

}
