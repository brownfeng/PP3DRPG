using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController 
{

    [Header("Skill")]
    public float kickOff = 20;

    /// <summary>
    /// Animation Event, 当兽人攻击对象时, 会在动画帧间触发这个回调
    /// </summary>
    public void KickOff()
    {
        if (attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);

            Vector3 direction = attackTarget.transform.position - transform.position; ;
            direction.Normalize();

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickOff;

            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
