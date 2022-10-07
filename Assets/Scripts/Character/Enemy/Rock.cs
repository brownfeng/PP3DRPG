using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{

    // 为了让石头能攻击玩家, 也能攻击石头人, 因此需要关键状态
    /// <summary>
    /// 石头拥有3个状态:
    /// 1. HitPlayer: 石头人扔出的石头, 目标是Player, 在CollisionEnter (因为石头拥有刚体.) 时, 判断
    /// 2. HitEnemy: 是 Player 盾反时的功能.
    /// 3. HitNothing: 是石头落地时的状态
    /// </summary>
    public enum RockState { HitPlayer, HitEnemy, HitNothing }
    // 石头拥有刚体, 刚体会在
    private Rigidbody rb;

    // 石头什么时候状态成 HitNothing?
    // 使用 RB.velocity 判断, 用刚体, 物理系统!!! 需要用 fixUp();
    public RockState rockState;

    public GameObject breakEffect; 

    [Header("Basic Settings")]
    public float force;

    public int damage;
    public GameObject target;
    private Vector3 direction;

    // Rock 是通过 Instantiate 方法创建的,
    // 因此在 Update 第一次调用之前, 先调用一次 Start() 方法
    // 由于这个对象, 并不是一开始存在于Scene中
    private void Start()
    {
        rockState = RockState.HitPlayer;

        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one; // 为了防止物理速度小于1f时, 状态为 HitNothing 消失...
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        // 这里用 1 这个阈值, 是因为很多时, 石头不会完全静止
        // 并且石头在生成时, 初始速度是0, 可能直接停止了... 需要给一个初始速度
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockState = RockState.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        // 调用这个方法的时机, 有可能是在Unity Loop的
        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    // 在发生碰撞时更新, 这里可能需要根据石头本身的状态去更新
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
