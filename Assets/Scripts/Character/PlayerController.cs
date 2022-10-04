using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private GameObject attackTarget;
    private float lastAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        //MouseManager.Instance.OnMouseClick += OnMouseClick; // 因为这里的 MouseManager.Instance 可能为空
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClick += MoveToTarget;
        MouseManager.Instance.OnAttackClick += EventAttack;
    }

    private void Update()
    {
        lastAttackTime -= Time.deltaTime;
        SwitchAnimation();
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
    }

    /// <summary>
    /// 鼠标点击事件, 点击地板, 触发移动到 target
    /// </summary>
    /// <param name="target"></param>
    private void MoveToTarget(Vector3 target) {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = target;
    }

    /// <summary>
    /// 鼠标点击事件, 点击攻击怪物, 先锁定目标, 开启协程持续处理锁定的目标
    /// </summary>
    /// <param name="target"></param>
    private void EventAttack(GameObject target)
    {
        if (target != null)
        {
            StopAllCoroutines();

            attackTarget = target;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        // 协程判断动作之前, 先判断角色是否停止了移动
        agent.isStopped = false;

        // 1. 先将 player 转向, 朝向 enemy
        // 2. 启动协程, 判断 player 与 enemy 距离
        transform.LookAt(attackTarget.transform);

        // FIXME: 距离判断, 后续根据武器大小, 怪物种类不同进行调整(有的怪物体积很大, 没办法完全走到距离为 1, 会发生碰撞)
        // 持续判断 player/enmey 的距离, 如果大于距离为攻击距离! 继续移动!!! 否则, 触发攻击动画
        while(Vector3.Distance(transform.position, attackTarget.transform.position) > 1)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        // 执行攻击: 先停止移动, 判断计时器(CD), 播放攻击动画
        agent.isStopped = true;

        if(lastAttackTime < 0)
        {
            // 攻击的动画基本都使用 Trigger:
            // 在切换到攻击动画时: 不需要 TransitionTime
            // 在Exit 攻击动画时, 需要 hasExit Time, 并且需要退出动画时间为1(一般来说,将攻击动画播放完成)
            anim.SetTrigger("Attack");

            // TODO: 重置冷却时间, 后续配置
            lastAttackTime = 0.5f;

        }
    }



}
