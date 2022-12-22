using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 targetPos;

    private void Start()
    {
        targetPos = transform.position;
    }

    private void Update()
    {
        MouseInput();
        Moving();
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            print(Input.mousePosition);
            targetPos = new Vector2(10f, 10f);
        }
    }

    private void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime);
    }
}
