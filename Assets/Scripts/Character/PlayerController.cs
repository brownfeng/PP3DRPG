using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;

    // Player 主动攻击的 GO
    private GameObject attackTarget;
    private float lastAttackTime;

    private bool isDead;

    /// <summary>
    /// 缓存默认的 stopDistance
    /// </summary>
    private float stopDistance;

    private void Awake()
    {
        // MouseManager.Instance.OnMouseClick += OnMouseClick; 
        // 因为这里的 MouseManager.Instance 可能为空
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    /// <summary>
    /// 注意, OnEnable/OnDisable  中注册/反注册 MouseManager 回调, 可以很方便我们在场景中
    /// 1. 先隐藏Player, 在游戏启动以后再打开角色!!! 方便调试
    /// 2. 在切换场景时, 需要重新生成新的Player, 这样注册的话也很方便
    /// 3. 请注意在 OnDisable 中会先判断 MouseManager.IsInitialized 是否初始化完成!!!
    /// </summary>
    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClick += MoveToTarget;
        MouseManager.Instance.OnAttackClick += EventAttack;

        // 在Player Start 时, 在GameManager中注册唯一的Player
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void OnDisable()
    {
        // 注意, 有可能手动情况, MouseManager 没有生成
        if (!MouseManager.IsInitialized)
            return;
        MouseManager.Instance.OnMouseClick -= MoveToTarget;
        MouseManager.Instance.OnAttackClick -= EventAttack;
    }

    /// <summary>
    /// 在Start() 方法中, 将 Player 注册到GameManager中
    /// </summary>
    private void Start()
    {
        // 如果需要自己测试, 可以打败这一行, 实际使用时候, 应该放在 OnEnable() 中
        // GameManager.Instance.RegisterPlayer(characterStats);

        // 角色一开始就去获取自己角色的内容
        SaveManager.Instance.LoadPlayerData();
    }

    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    /// <summary>
    /// 鼠标点击事件, 点击地板, 触发移动到 target
    /// </summary>
    /// <param name="target"></param>
    private void MoveToTarget(Vector3 target)
    {

        StopAllCoroutines();
        if (isDead)
        {
            return;
        }
        // 当点击移动时, 能100%确保移动到指定位置, 而点击攻击角色时, 需要设置成CharacterData.attackRange
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    /// <summary>
    /// 鼠标点击事件, 点击攻击怪物, 先锁定目标, 开启协程持续处理锁定的目标
    /// </summary>
    /// <param name="target"></param>
    private void EventAttack(GameObject target)
    {
        if (isDead)
        {
            return;
        }

        if (target != null)
        {
            Debug.Log($"target {target}");
            attackTarget = target;
            // 计算Player 当前的攻击是否是暴击
            characterStats.isCritical = Random.value < characterStats.CriticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        // 协程判断动作之前, 先判断角色是否停止了移动
        agent.isStopped = false;
        /// 当攻击一些角色时, 如果角色模型很大. 可能攻击不到...
        agent.stoppingDistance = characterStats.attackData.attackRange;

        // 1. 先将 player 转向, 朝向 enemy
        // 2. 启动协程, 判断 player 与 enemy 距离
        transform.LookAt(attackTarget.transform);

        // FIXME: 距离判断, 后续根据武器大小, 怪物种类不同进行调整(有的怪物体积很大, 没办法完全走到距离为 1, 会发生碰撞)
        // 持续判断 player/enmey 的距离, 如果大于距离为攻击距离! 继续移动!!! 否则, 触发攻击动画
        while (Vector3.Distance(transform.position, attackTarget.transform.position) > characterStats.AttackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        // 执行攻击: 先停止移动, 判断计时器(CD), 播放攻击动画
        agent.isStopped = true;

        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);

            // 攻击的动画基本都使用 Trigger:
            // 在切换到攻击动画时: 不需要 TransitionTime
            // 在Exit 攻击动画时, 需要 hasExit Time, 并且需要退出动画时间为1(一般来说,将攻击动画播放完成)
            anim.SetTrigger("Attack");

            // 重置冷却时间, 后续配置
            lastAttackTime = characterStats.CoolDown;
        }
    }

    // Animator Event, 主动触发暴击动画, 会让 targetStats 发生后仰动画
    // 这里认为角色既可以攻击石头, 也可以攻击石头, 在攻击石头的时, 类似盾反效果
    // 基于这种理论, 我们需要分一个大类, Attackable 标签给石头, 这里就能判断究竟攻击的是什么
    private void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockState.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            // Player 是主动攻击怪物, 因此一定拥有 target
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
