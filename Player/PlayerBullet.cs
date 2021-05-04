using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float speed;
    private Rigidbody bullet;

    Enemy enemy;
    PlayerStatus player;

    void Start()
    {
        gameObject.tag = "PlayerBullet";
        GameObject ThePlayer = GameObject.Find("Player");
        PlayerStatus player = ThePlayer.GetComponent<PlayerStatus>();
        GameObject TheEnemy = GameObject.Find("Enemy");
        Enemy enemy = TheEnemy.GetComponent<Enemy>();

    }

    void Update()
    {
        bullet.AddForce(Vector3.forward * Time.deltaTime * speed);
    }

    void doDamage()
    {
        PlayerStatus.PlayerHP = PlayerStatus.PlayerHP - damage;
        Enemy.EnemyHP = Enemy.EnemyHP - damage;
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision bhitbox)
    {
        if (bhitbox.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
        else
            doDamage();
    }
}