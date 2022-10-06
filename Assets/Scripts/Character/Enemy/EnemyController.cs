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
    // ����� Patrol ����, ��������Ѳ�߷�Χ
    public float patrolRange;

    private Vector3 nextWayPos;

    // ��ʼλ��
    private Vector3 guardPos;
    // ��ʼ��ת
    private Quaternion guardRotation;

    [Header("Look At Time")]
    // Ѹ�ν�ɫ�ڵ���ָ��λ���Ժ�, ��ȴ�һ��ʱ��, �ٽ�����һ��λ��
    public float lookAtTime;
    private float remainLookAtTime;
    [Header("Attack Cool Down Time")]
    // �������, �ж�(��Ҫ�� Update() �����м���)
    private float lastAttackTime;

    private Animator anim;
    private NavMeshAgent agent;
    protected CharacterStats characterStats;
    private Collider coll;

    // ����⵽ Player ��ָ����Χ, ��ʼ�л� FSM
    protected GameObject attackTarget;
    // ��¼ԭ���ٶ�(Ĭ����2.5), �뿪 Chase ״̬ʱ, speed * 0.5.
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer ��, ��������Enemy�Ƿ������߶��� => ֻ�� Patrol State ��Ч
    private bool isChase; // �Ƿ��л��� Attack Layer. ������һ��״̬��ʲô, ���л�����ǰLayer
    private bool isFollow; //  �� Attack Layer ����Ч, ��ʾ�Ƿ���׷��Player( Run ����)
    private bool isDeath; // �� Death Layer ����Ч, ��ʾ��ǰ��ɫ��

    private bool playerDead;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        // ����ʱ, �����ʼλ��. ������ Patrol ʱ, Χ�Ƴ�ʼ��ΧѲ��
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

    // ����ű�֮��� OnEnable �� Start �����޷���֤˳��, ֻ��ͳһ�����ʵ��֮���ܱ�֤˳��
    // �����Ҫǿ�Ʊ�֤ʵ��˳��, Edit > Project Settings > Script Execution Order�Խű��������е�˳�������
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
    /// ÿһ֡�������� Animator ���¼�, ��������״̬���仯(ע��ÿһ֡!!!)
    /// ����� SwitchState() ������, ÿһ֡��Ҫ�����⼸��״ֵ̬
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
            // �������Player, �л��� ChaseState
            enemyState = EnemyState.CHASE;
        }

        switch (enemyState)
        {
            case EnemyState.GUARD:
                isChase = false;

                // FIXME: �����һֱִ�е�ǰ����
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

                // �ж��Ƿ��� nextWayPoint
                if (Vector3.Distance(transform.position, nextWayPos) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    // ��ȴʱ��
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

                // TODO: �ڹ�����Χ��, ����
                // TODO: ����Trigger ����
                // �ж��Ƿ�����Χ
                if (!FoundPlayer())
                {
                    // TODO: ����ս����һ��״̬
                    isFollow = false;

                    // ���������һЩ�ȴ�ʱ��, �ȵȴ�!!!
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
                    // TODO: ��Ҫ���� attackTarget
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //TODO: ������Χ���, ����ڷ�Χ, ���й���, ��������ȴʱ��
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    // TODO: ���¼��㱩����
                    // TODO: �ж� CoolDown ʱ���, ��������
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.CoolDown;

                        // ���� Enemy �ı���״̬
                        characterStats.isCritical = Random.value < characterStats.CriticalChance;

                        // ִ�й���
                        Attack();
                    }
                }
                break;
            case EnemyState.DEAD:
                coll.enabled = false;
                /// ���ﲻҪʹ�� agnet.enable = false, ����� Animation Event��, ���ǻ���� StopAgent �ű�, �ýű����ȡ Enemy ���ϵ� NavMeshAgent, Ȼ�����
                /// ����ڹ�������ʱ, �ᵼ�±���, ��˸��õķ�����ʹ�� agent.radius
                //agent.enabled = false;
         
                agent.radius = 0;// ��������Ͳ���������ɫ��.  ��ʬ�����ڵ�.
                Destroy(gameObject, 2);
                break;
        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        // 1. ������

        if(TargetInAttackRange())
        {
            anim.SetTrigger("Attack");
        }

        if (TargetInSkillRange())
        {
            anim.SetTrigger("Skill");
        }
        // 2. ���ܹ���
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

    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        // ע��: ����ѡ��ķ�Χ, ��ʹ�� guardPos + random, ������Y����ʹ�ù��ܵ� ģ�͵�ǰYλ��!!(�������¶�)
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        // ע��: ����ѡ��ĵ�, ���޷� NavMesh ��...
        // ��NavMesh ��,�� randomPoint Ϊ����, patrolRange Ϊ��Χ, ѡ��һ������
        NavMeshHit hit;
        // �ҵ���Ŀ�������ķ�Χ�ڵĵ�����, �����Ƿ����ҵ�
        bool walkable = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1);
        nextWayPos = walkable ? hit.position : transform.position;
    }

    // Animator Event - ���ڱ��������м䴥���¼�, �¼��������� Player ���ϵĶ���
    private void Hit()
    {
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            // Player��������������, ���һ��ӵ�� target
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // �������ɫ, ��ѡ�н�ɫ��ʱ, ���� Gizmos
    // 1. ���� ��Ұ��Χ
    // 2. ���� Ѳ�߷�Χ - ʹ�� WireSphere ������ Sphere(����Բ...)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }

    public void EndNotify()
    {
        // ��ʤ����
        // ֹͣ���е��ƶ�
        // ֹͣAgent
        playerDead = true;
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
