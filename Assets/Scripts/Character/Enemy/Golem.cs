using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController 
{

    [Header("Skill")]
    public float kickForce = 20;

    public GameObject rockPrefab;

    public Transform handPos;
    /// <summary>
    /// Animation Event, �����˹�������ʱ, ���ڶ���֡�䴥������ص�
    /// </summary>
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;

            // ���ݸ���ϲ�����
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // Animation Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            // ��������һ�� Rock GO, ע�� position �� rotate λ��.
            // Ȼ������������� target����!!!
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            //  ���� Rock.RockScript �ű��е� target �ֶ�
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
