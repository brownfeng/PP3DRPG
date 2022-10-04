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
        //MouseManager.Instance.OnMouseClick += OnMouseClick; // ��Ϊ����� MouseManager.Instance ����Ϊ��
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
    /// ������¼�, ����ذ�, �����ƶ��� target
    /// </summary>
    /// <param name="target"></param>
    private void MoveToTarget(Vector3 target) {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = target;
    }

    /// <summary>
    /// ������¼�, �����������, ������Ŀ��, ����Э�̳�������������Ŀ��
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
        // Э���ж϶���֮ǰ, ���жϽ�ɫ�Ƿ�ֹͣ���ƶ�
        agent.isStopped = false;

        // 1. �Ƚ� player ת��, ���� enemy
        // 2. ����Э��, �ж� player �� enemy ����
        transform.LookAt(attackTarget.transform);

        // FIXME: �����ж�, ��������������С, �������಻ͬ���е���(�еĹ�������ܴ�, û�취��ȫ�ߵ�����Ϊ 1, �ᷢ����ײ)
        // �����ж� player/enmey �ľ���, ������ھ���Ϊ��������! �����ƶ�!!! ����, ������������
        while(Vector3.Distance(transform.position, attackTarget.transform.position) > 1)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        // ִ�й���: ��ֹͣ�ƶ�, �жϼ�ʱ��(CD), ���Ź�������
        agent.isStopped = true;

        if(lastAttackTime < 0)
        {
            // �����Ķ���������ʹ�� Trigger:
            // ���л�����������ʱ: ����Ҫ TransitionTime
            // ��Exit ��������ʱ, ��Ҫ hasExit Time, ������Ҫ�˳�����ʱ��Ϊ1(һ����˵,�����������������)
            anim.SetTrigger("Attack");

            // TODO: ������ȴʱ��, ��������
            lastAttackTime = 0.5f;

        }
    }



}
