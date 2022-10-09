using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{

    // Ϊ����ʯͷ�ܹ������, Ҳ�ܹ���ʯͷ��, �����Ҫ�ؼ�״̬
    /// <summary>
    /// ʯͷӵ��3��״̬:
    /// 1. HitPlayer: ʯͷ���ӳ���ʯͷ, Ŀ����Player, ��CollisionEnter (��Ϊʯͷӵ�и���.) ʱ, �ж�
    /// 2. HitEnemy: �� Player �ܷ�ʱ�Ĺ���.
    /// 3. HitNothing: ��ʯͷ���ʱ��״̬
    /// </summary>
    public enum RockState { HitPlayer, HitEnemy, HitNothing }
    // ʯͷӵ�и���, �������
    private Rigidbody rb;

    // ʯͷʲôʱ��״̬�� HitNothing?
    // ʹ�� RB.velocity �ж�, �ø���, ����ϵͳ!!! ��Ҫ�� fixUp();
    public RockState rockState;

    public GameObject breakEffect; 

    [Header("Basic Settings")]
    public float force;

    public int damage;
    public GameObject target;
    private Vector3 direction;

    // Rock ��ͨ�� Instantiate ����������,
    // ����� Update ��һ�ε���֮ǰ, �ȵ���һ�� Start() ����
    // �����������, ������һ��ʼ������Scene��
    private void Start()
    {
        rockState = RockState.HitPlayer;

        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one; // Ϊ�˷�ֹ�����ٶ�С��1fʱ, ״̬Ϊ HitNothing ��ʧ...
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        // ������ 1 �����ֵ, ����Ϊ�ܶ�ʱ, ʯͷ������ȫ��ֹ
        // ����ʯͷ������ʱ, ��ʼ�ٶ���0, ����ֱ��ֹͣ��... ��Ҫ��һ����ʼ�ٶ�
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockState = RockState.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        // �������������ʱ��, �п�������Unity Loop��
        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    // �ڷ�����ײʱ����, ���������Ҫ����ʯͷ�����״̬ȥ����
    private void OnCollisionEnter(Collision other)
    {
        switch (rockState)
        {
            case RockState.HitPlayer:
                if (other.gameObject.CompareTag("Player")){
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");

                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());

                    rockState = RockState.HitNothing;
                }
                break;
            case RockState.HitEnemy:
                if (other.gameObject.GetComponent<Golem>()) {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
            
        }
    }
}
