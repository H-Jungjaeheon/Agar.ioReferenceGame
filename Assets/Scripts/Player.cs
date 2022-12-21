using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Vector3 targetPos;

    Vector3 mousePos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
            targetPos = Camera.main.ScreenToWorldPoint(mousePos);
        }
        MoveToTargetPos();
    }

    private void MoveToTargetPos()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, 10f);
    }

    private IEnumerator MoveToTarget()
    {

        yield return null;
    }
}
