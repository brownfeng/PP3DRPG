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
    // ��¼ԭ���ٶ�(Ĭ����2.5)
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer ��, ��������Enemy�Ƿ������߶���
    private bool isChase; // �Ƿ��л��� Attack Layer. ������һ��״̬��ʲô, ���л�����ǰLayer
    private bool isFollow; //  �� Attack Layer ����Ч, ��ʾ�Ƿ���׷��Player( Run ����)

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
        agent.speed = speed * 0.5f; // ����״̬, �ٶ�ֻ��ԭ����һ��

        // �������Player, �л��� ChaseState
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

                // TODO: �ڹ�����Χ��, ����
                // TODO: ����Trigger ����
                // �ж��Ƿ�����Χ
                if (!FoundPlayer())
                {
                    // ���ѻ���һ��״̬
                    isFollow = false;
                    agent.destination = transform.position;

                    enemyState = isGuard ?  EnemyState.GUARD: EnemyState.PATROL;
                }
                else {
                    // ׷��Player
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
        /// Physics ��API ����Ҫ���ܱ�����ӵ�� Collider, �����Ҫ���������� Collider
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
