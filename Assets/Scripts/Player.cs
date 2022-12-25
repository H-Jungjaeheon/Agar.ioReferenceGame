using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance = null;

    [Tooltip("이동속도")]
    public float speed;

    [Tooltip("현재 크기")]
    private float size;

    public float Size
    {
        get { return size; }
        set
        {
            size = value;

            sizeVector.x = size;
            sizeVector.y = size;
            sizeVector.z = 1;

            transform.localScale = sizeVector;
        }
    }

    private Vector3 sizeVector = new Vector3(0f, 0f, 1); //크기 조정용 벡터

    private Vector2 targetPos; //현재 마우스로 찍은 목표 포지션

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MouseInput();
        Moving();
    }
    private void Init()
    {
        if (instance == null)
        {
            instance = this;
        }
        size = 0.7f;
        sizeVector.x = size;
        sizeVector.y = size;
        targetPos = transform.position;
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            targetPos = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
    }

    private void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.gameObject.GetComponent<IInteraction>().Interaction();
    //}
}
