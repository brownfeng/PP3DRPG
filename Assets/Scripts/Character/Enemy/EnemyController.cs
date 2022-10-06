using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyState enemyState;

    [Header("Basic Settings")]
    public float sightRadius;

    [Header("Patrol Control")]
    public bool isGuard;
    // 如果是 Patrol 对象, 设置它的巡逻范围
    public float patrolRange;

    private Vector3 nextWayPos;

    // 初始位置
    private Vector3 guardPos;
    // 初始旋转
    private Quaternion guardRotation;

    [Header("Look At Time")]
    // 迅游角色在到达指定位置以后, 会等待一段时间, 再进入下一个位置
    public float lookAtTime;
    private float remainLookAtTime;
    [Header("Attack Cool Down Time")]
    // 攻击间隔, 判断(需要在 Update() 方法中减少)
    private float lastAttackTime;

    private Animator anim;
    private NavMeshAgent agent;
    protected CharacterStats characterStats;
    private Collider coll;

    // 当检测到 Player 在指定范围, 开始切换 FSM
    protected GameObject attackTarget;
    // 记录原有速度(默认是2.5), 离开 Chase 状态时, speed * 0.5.
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer 中, 用来描述Enemy是否在行走动画 => 只在 Patrol State 有效
    private bool isChase; // 是否切换到 Attack Layer. 无论上一个状态是什么, 会切换到当前Layer
    private bool isFollow; //  在 Attack Layer 中有效, 表示是否在追击Player( Run 动画)
    private bool isDeath; // 在 Death Layer 中有效, 表示当前角色在

    private bool playerDead;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        // 启动时, 缓存初始位置. 后续再 Patrol 时, 围绕初始范围巡逻
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        speed = agent.speed;
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyState = EnemyState.GUARD;
        }
        else
        {
            enemyState = EnemyState.PATROL;
            GetNewWayPoint();
        }        
    }

    // 多个脚本之间的 OnEnable 与 Start 调用无法保证顺序, 只有统一个组件实例之间能保证顺序
    // 如果需要强制保证实例顺序, Edit > Project Settings > Script Execution Order对脚本进行运行的顺序调整。
    void OnEnable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.AddObserver(this);
        }
    }

    void OnDisable()
    {
        if(!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }


    // Update is called once per frame
    void Update()
    {
        if (characterStats.CurrentHealth == 0)
        {
            isDeath = true;
        }

        if(!playerDead)
        {
            SwitchState();
            SetAnimator();

            lastAttackTime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 每一帧都会设置 Animator 的事件, 驱动动画状态机变化(注意每一帧!!!)
    /// 因此在 SwitchState() 方法中, 每一帧都要设置这几个状态值
    /// </summary>
    private void SetAnimator()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDeath);
    }

    private void SwitchState() {

        if (isDeath)
        {
            enemyState = EnemyState.DEAD;
        }
        else if (FoundPlayer())
        {
            // 如果发现Player, 切换到 ChaseState
            enemyState = EnemyState.CHASE;
        }

        switch (enemyState)
        {
            case EnemyState.GUARD:
                isChase = false;

                // FIXME: 这里会一直执行当前函数
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.05f);
                    }
                } 
     
                break;
            case EnemyState.PATROL:
                isWalk = true;
                isChase = false;

                agent.speed = speed * 0.5f;

                // 判断是否到了 nextWayPoint
                if (Vector3.Distance(transform.position, nextWayPos) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    // 冷却时间
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    } else
                    {
                        GetNewWayPoint();
                    }
                }
                else
                {
                    isWalk = true;
                    agent.destination = nextWayPos;
                }
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
                    // TODO: 拉脱战回上一个状态
                    isFollow = false;

                    // 如果还存在一些等待时间, 先等待!!!
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                    {
                        enemyState = EnemyState.GUARD;
                    }
                    else {
                        enemyState = EnemyState.PATROL;
                    }
                }
                else
                {
                    // TODO: 需要跟踪 attackTarget
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //TODO: 攻击范围检测, 如果在范围, 进行攻击, 并重置冷却时间
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    // TODO: 重新计算暴击率
                    // TODO: 判断 CoolDown 时间戳, 触发攻击
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.CoolDown;

                        // 更新 Enemy 的暴击状态
                        characterStats.isCritical = Random.value < characterStats.CriticalChance;

                        // 执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyState.DEAD:
                coll.enabled = false;
                /// 这里不要使用 agnet.enable = false, 因此在 Animation Event中, 我们会调用 StopAgent 脚本, 该脚本会获取 Enemy 身上的 NavMeshAgent, 然后操作
                /// 因此在怪物死亡时, 会导致报错, 因此更好的方法是使用 agent.radius
                //agent.enabled = false;
         
                agent.radius = 0;// 这样怪物就不会阻塞角色了.  卡尸不存在的.
                Destroy(gameObject, 2);
                break;
        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        // 1. 近身攻击

        if(TargetInAttackRange())
        {
            anim.SetTrigger("Attack");
        }

        if (TargetInSkillRange())
        {
            anim.SetTrigger("Skill");
        }
        // 2. 技能攻击
    }

    private bool TargetInAttackRange()
    {
        if(attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.AttackRange;
        }else
        {
            return false;
        }
    }

    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.SkillRange;
        }
        else
        {
            return false;
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

    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        // 注意: 这里选择的范围, 是使用 guardPos + random, 并且在Y方向使用功能的 模型当前Y位置!!(可能有坡度)
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        // 注意: 可能选择的点, 是无法 NavMesh 的...
        // 在NavMesh 中,以 randomPoint 为坐标, patrolRange 为范围, 选择一个坐标
        NavMeshHit hit;
        // 找到与目标点最近的范围内的导航点, 返回是否能找到
        bool walkable = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1);
        nextWayPos = walkable ? hit.position : transform.position;
    }

    // Animator Event - 会在暴击动画中间触发事件, 事件用来出发 Player 身上的动画
    private void Hit()
    {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            // Player是主动攻击怪物, 因此一定拥有 target
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // 当点击角色, 并选中角色的时, 绘制 Gizmos
    // 1. 绘制 视野范围
    // 2. 绘制 巡逻范围 - 使用 WireSphere 而不是 Sphere(空心圆...)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }

    public void EndNotify()
    {
        // 获胜动画
        // 停止所有的移动
        // 停止Agent
        playerDead = true;
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
