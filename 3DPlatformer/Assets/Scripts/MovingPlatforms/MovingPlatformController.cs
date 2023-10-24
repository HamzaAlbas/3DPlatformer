using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : MonoBehaviour
{
    public Transform[] movePoints;
    public float speed = 2.0f;
    public bool loop = true;

    private int currentPointIndex = 0;
    private Vector3 currentTarget;
    public bool isMoving = false;

    void Start()
    {
        SetNextTarget();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
    }

    void SetNextTarget()
    {
        if (movePoints.Length == 0)
            return;

        currentTarget = movePoints[currentPointIndex].position;
    }

    void MoveToTarget()
    {
        if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
        {
            if (loop)
            {
                currentPointIndex = (currentPointIndex + 1) % movePoints.Length;
                SetNextTarget();
            }
            else
            {
                if (currentPointIndex < movePoints.Length - 1)
                {
                    currentPointIndex++;
                    SetNextTarget();
                }
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
    }

    public void StartMoving()
    {
        isMoving = true;
        SetNextTarget();
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isMoving)
            {
                other.transform.SetParent(transform);
            }
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
