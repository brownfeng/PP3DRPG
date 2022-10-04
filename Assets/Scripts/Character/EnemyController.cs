using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private EnemyState enemyState;

    [Header("Basic Settings")]
    public float sightRadius;

    [Header("Patrol Control")]
    public bool isGuard;
    // 如果是 Patrol 对象, 设置它的巡逻范围
    public float patrolRange;

    [SerializeField] private Vector3 nextWayPos;
    [SerializeField] private Vector3 guardPos;

    private Animator anim;
    private NavMeshAgent agent;

    // 当检测到 Player 在指定范围, 开始切换 FSM
    private GameObject attackTarget;
    // 记录原有速度(默认是2.5), 离开 Chase 状态时, speed * 0.5.
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer 中, 用来描述Enemy是否在行走动画 => 只在 Patrol State 有效
    private bool isChase; // 是否切换到 Attack Layer. 无论上一个状态是什么, 会切换到当前Layer
    private bool isFollow; //  在 Attack Layer 中有效, 表示是否在追击Player( Run 动画)

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;

        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (isGuard) {
            enemyState = EnemyState.GUARD;

        }
        else 
        {
            enemyState = EnemyState.PATROL;
            GetNewWayPoint();
        }
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
                isWalk = true;
                isChase = false;

                agent.speed = speed * 0.5f;

                // 判断是否到 nextWayPoint
                if(Vector3.Distance(transform.position, nextWayPos) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    GetNewWayPoint();
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

    private void GetNewWayPoint()
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        // 寻找 position 同一个Y 高度的随机值!
        Vector3 randomPoint = new Vector3(randomX, transform.position.y, randomZ);
        //FIXME: 可能出现问题 - 可能选择的点, 是无法走过去的...
        // 在NavMesh 中,以 randomPoint 为坐标, patrolRange 为范围, 选择一个坐标
        NavMeshHit hit;
        // 找到与目标点最近的范围内的导航点, 返回是否能找到
        bool walkable = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1);
        nextWayPos = walkable ? hit.position : transform.position;
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
}
