using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Vector3 targetPos;

    private void Update()
    {
        Get_MouseInput();
        Update_Moving();
    }

    private void Get_MouseInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePos = Input.mousePosition;
            targetPos = mousePos;
        }
    }

    private void Update_Moving()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, 0.1f);
    }

}
