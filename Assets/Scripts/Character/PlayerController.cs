using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        //MouseManager.Instance.OnMouseClick += OnMouseClick; // ��Ϊ����� MouseManager.Instance ����Ϊ��
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClick += MoveToTarget;
    }

    private void Update()
    {
        SwitchAnimation();
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
    }

    private void MoveToTarget(Vector3 vector) {
        agent.destination = vector;
    }

}
