using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{

    [Header("Skill")]
    public float kickForce = 20;
    
    /// <summary>
    /// Animation Event, �����˹�������ʱ, ���ڶ���֡�䴥������ص�
    /// </summary>
    public void KickOff()
    {
        if(attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);

            Vector3 direction = attackTarget.transform.position - transform.position; ;
            direction.Normalize();

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
