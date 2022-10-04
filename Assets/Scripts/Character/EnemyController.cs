using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public EnemyState enemyState;

    [Header("Basic Settings")]
    public float sightRadius;
  
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    // Update is called once per frame
    void Update()
    {
        SwitchState();
    }

    private void SwitchState() {

        // 如果发现Player, 切换到 ChaseState
        if (FoundPlayer())
        {
            enemyState = EnemyState.CHASE;
            Debug.Log("Found player");
        }

        switch (enemyState)
        {
            case EnemyState.GUARD:
                break;
            case EnemyState.PATROL:
                break;
            case EnemyState.CHASE:
                break;
            case EnemyState.DEAD:
                break;
        }
    }

    private bool FoundPlayer()
    {
        /// Physics 的API 都会要求周边物体拥有 Collider, 因此需要给物体增加 Collider
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders)
        {
            if(target.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}
