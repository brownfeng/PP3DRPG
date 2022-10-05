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
    // ����� Patrol ����, ��������Ѳ�߷�Χ
    public float patrolRange;

    [SerializeField] private Vector3 nextWayPos;
    [SerializeField] private Vector3 guardPos;
    [SerializeField] private Quaternion guardRotation;

    [Header("Look At Time")]
    // Ѹ�ν�ɫ�ڵ���ָ��λ���Ժ�, ��ȴ�һ��ʱ��, �ٽ�����һ��λ��
    public float lookAtTime;
    private float remainLookAtTime;
    [Header("Attack Cool Down Time")]
    // �������, �ж�(��Ҫ�� Update() �����м���)
    private float lastAttackTime;

    private Animator anim;
    private NavMeshAgent agent;
    private CharacterStats characterStats;

    // ����⵽ Player ��ָ����Χ, ��ʼ�л� FSM
    private GameObject attackTarget;
    // ��¼ԭ���ٶ�(Ĭ����2.5), �뿪 Chase ״̬ʱ, speed * 0.5.
    private float speed;

    // Animator Layer State
    private bool isWalk; // BaseLayer ��, ��������Enemy�Ƿ������߶��� => ֻ�� Patrol State ��Ч
    private bool isChase; // �Ƿ��л��� Attack Layer. ������һ��״̬��ʲô, ���л�����ǰLayer
    private bool isFollow; //  �� Attack Layer ����Ч, ��ʾ�Ƿ���׷��Player( Run ����)

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        // ����ʱ, �����ʼλ��. ������ Patrol ʱ, Χ�Ƴ�ʼ��ΧѲ��
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        speed = agent.speed;
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

        lastAttackTime -= Time.deltaTime; 
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
    }

    private void SwitchState() {
        agent.speed = speed * 0.5f; // ����״̬, �ٶ�ֻ��ԭ����һ��

        // �������Player, �л��� ChaseState
        if (FoundPlayer())
        {
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

                    Debug.Log(" λ�Ӳ���׼.. ��û��, ����ֹͣ��");
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

                        // �����ж�
                        characterStats.isCritical = Random.value < characterStats.CriticalChance;

                        // ִ�й���
                        Attack();
                    }
                }
                break;
            case EnemyState.DEAD:
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

    // Animator Event
    private void Hit()
    {
        Debug.Log("Animation Event Hit...");
        if(attackTarget != null)
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
}
