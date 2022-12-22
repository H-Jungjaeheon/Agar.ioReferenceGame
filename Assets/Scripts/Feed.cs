using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour, IHit
{
    [SerializeField]
    private float Value = 0.1f;
    void Start()
    {

    }

    public void Hit()
    {
        Player.instance.Size += Value;
        Destroy(gameObject);
    }
}
