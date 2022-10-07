using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;

    // Player ���������� GO
    private GameObject attackTarget;
    private float lastAttackTime;

    private bool isDead;

    /// <summary>
    /// ����Ĭ�ϵ� stopDistance
    /// </summary>
    private float stopDistance;

    private void Awake()
    {
        // MouseManager.Instance.OnMouseClick += OnMouseClick; 
        // ��Ϊ����� MouseManager.Instance ����Ϊ��
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClick += MoveToTarget;
        MouseManager.Instance.OnAttackClick += EventAttack;

        // ��Player Start ʱ, ��GameManager��ע��Ψһ��Player
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if(isDead)
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
    /// ������¼�, ����ذ�, �����ƶ��� target
    /// </summary>
    /// <param name="target"></param>
    private void MoveToTarget(Vector3 target) {

        StopAllCoroutines();
        if (isDead)
        {
            return;
        }
        // ������ƶ�ʱ, ��100%ȷ���ƶ���ָ��λ��, �����������ɫʱ, ��Ҫ���ó�CharacterData.attackRange
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    /// <summary>
    /// ������¼�, �����������, ������Ŀ��, ����Э�̳�������������Ŀ��
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
            // ����Player ��ǰ�Ĺ����Ƿ��Ǳ���
            characterStats.isCritical = Random.value < characterStats.CriticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        // Э���ж϶���֮ǰ, ���жϽ�ɫ�Ƿ�ֹͣ���ƶ�
        agent.isStopped = false;
        /// ������һЩ��ɫʱ, �����ɫģ�ͺܴ�. ���ܹ�������...
        agent.stoppingDistance = characterStats.attackData.attackRange;

        // 1. �Ƚ� player ת��, ���� enemy
        // 2. ����Э��, �ж� player �� enemy ����
        transform.LookAt(attackTarget.transform);

        // FIXME: �����ж�, ��������������С, �������಻ͬ���е���(�еĹ�������ܴ�, û�취��ȫ�ߵ�����Ϊ 1, �ᷢ����ײ)
        // �����ж� player/enmey �ľ���, ������ھ���Ϊ��������! �����ƶ�!!! ����, ������������
        while(Vector3.Distance(transform.position, attackTarget.transform.position) > characterStats.AttackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        // ִ�й���: ��ֹͣ�ƶ�, �жϼ�ʱ��(CD), ���Ź�������
        agent.isStopped = true;

        if(lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);

            // �����Ķ���������ʹ�� Trigger:
            // ���л�����������ʱ: ����Ҫ TransitionTime
            // ��Exit ��������ʱ, ��Ҫ hasExit Time, ������Ҫ�˳�����ʱ��Ϊ1(һ����˵,�����������������)
            anim.SetTrigger("Attack");

            // ������ȴʱ��, ��������
            lastAttackTime = characterStats.CoolDown;
        }
    }

    // Animator Event, ����������������, ���� targetStats ������������
    // ������Ϊ��ɫ�ȿ��Թ���ʯͷ, Ҳ���Թ���ʯͷ, �ڹ���ʯͷ��ʱ, ���ƶܷ�Ч��
    // ������������, ������Ҫ��һ������, Attackable ��ǩ��ʯͷ, ��������жϾ�����������ʲô
    private void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if(attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockState.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }else {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            // Player ��������������, ���һ��ӵ�� target
            targetStats.TakeDamage(characterStats, targetStats);
        }
    } 
}
