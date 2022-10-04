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

    private Animator anim;
    private NavMeshAgent agent;

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
                isWalk = true;
                isChase = false;

                agent.speed = speed * 0.5f;

                // �ж��Ƿ� nextWayPoint
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

    private void GetNewWayPoint()
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        // Ѱ�� position ͬһ��Y �߶ȵ����ֵ!
        Vector3 randomPoint = new Vector3(randomX, transform.position.y, randomZ);
        //FIXME: ���ܳ������� - ����ѡ��ĵ�, ���޷��߹�ȥ��...
        // ��NavMesh ��,�� randomPoint Ϊ����, patrolRange Ϊ��Χ, ѡ��һ������
        NavMeshHit hit;
        // �ҵ���Ŀ�������ķ�Χ�ڵĵ�����, �����Ƿ����ҵ�
        bool walkable = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1);
        nextWayPos = walkable ? hit.position : transform.position;
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
