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

    public bool isGuard;
  
    private NavMeshAgent agent;

    private GameObject attackTarget;
    // 记录原有速度(默认是2.5)
    private float speed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;
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
                agent.speed = speed;

                // TODO: 在攻击范围内, 攻击
                // TODO: 攻击Trigger 动画
                // 判断是否在周围
                if (!FoundPlayer())
                {
                    // 拉脱回上一个状态
                }
                else {
                    // 追击Player
                    agent.destination = attackTarget.transform.position;
                    agent.speed = speed * 0.5f;
                }
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
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }
}
