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
    /// Animation Event, 当兽人攻击对象时, 会在动画帧间触发这个回调
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

            // 根据个人喜好添加
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    // Animation Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            // 主动创建一个 Rock GO, 注意 position 和 rotate 位置.
            // 然后给它设置它的 target属性!!!
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            //  设置 Rock.RockScript 脚本中的 target 字段
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
