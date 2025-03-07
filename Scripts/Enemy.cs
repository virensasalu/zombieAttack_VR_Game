using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector3 direction;
    public Player player;

    public int damage = 3;
    public float speed = 3.5f;
    public float distanceToStop = 1.5f;
    public bool chasingPlayer = true;

    public float eatingInterval = 0.5f;
    private float eatingTimer = 0f;

    void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < distanceToStop)
        {
            chasingPlayer = false;
        }

        if (chasingPlayer)
        {
            // Move towards the player
            transform.position += direction * speed * Time.deltaTime;
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        }
        else
        {
            // Attack the player
            eatingTimer -= Time.deltaTime;
            if (eatingTimer <= 0f)
            {
                eatingTimer = eatingInterval;
                player.health -= damage;
            }
        }
    }
}
