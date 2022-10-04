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

    private Animator anim;
    private NavMeshAgent agent;

    private GameObject attackTarget;
    // 记录原有速度(默认是2.5)
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer 中, 用来描述Enemy是否在行走动画
    private bool isChase; // 是否切换到 Attack Layer. 无论上一个状态是什么, 会切换到当前Layer
    private bool isFollow; //  在 Attack Layer 中有效, 表示是否在追击Player( Run 动画)

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;

        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        SwitchState();
        SetAnimator();
    }

    private void SetAnimator()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
    }

    private void SwitchState() {
        agent.speed = speed * 0.5f; // 正常状态, 速度只有原来的一半

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
                isWalk = false;
                isChase = true;

                agent.speed = speed;

                // TODO: 在攻击范围内, 攻击
                // TODO: 攻击Trigger 动画
                // 判断是否在周围
                if (!FoundPlayer())
                {
                    // 拉脱回上一个状态
                    isFollow = false;
                    agent.destination = transform.position;

                    enemyState = isGuard ?  EnemyState.GUARD: EnemyState.PATROL;
                }
                else {
                    // 追击Player
                    agent.destination = attackTarget.transform.position;
                    isFollow = true;
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
