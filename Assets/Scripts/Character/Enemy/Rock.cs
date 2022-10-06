using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Basic Settings")]
    public float force;

    public GameObject target;
    private Vector3 direction;

    // Rock 是通过 Instantiate 方法创建的, 因此在 Update 第一次调用之前, 先调用一次 Start() 方法
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        FlyToTarget();
    }

    public void FlyToTarget()
    {
        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }

        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}
