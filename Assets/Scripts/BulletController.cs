using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] float speed;

    private bool isParried = false;
    private Vector3 velocity;
    private float lifeSpan = 10f;

    private void Start()
    {
        velocity = new Vector3(speed, 0f);

        // Destroy after too long
        StartCoroutine(Kill(lifeSpan));

    }

    private void Update()
    {
        transform.position += velocity;
    }

    public void Reflect()
    {
        isParried = true;
        velocity = -velocity * 2;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Collide with player
        PlayerController player = collision.GetComponent<PlayerController>();
        if (!isParried && player != null)
        {
            if (player.canReflect)
            {
                Reflect();
                player.ActivateParry();
            }
            else
            {
                player.TakeDamage();
                Destroy(gameObject);
            }
            return;
        }

        TurretController turret = collision.GetComponent<TurretController>();
        if (isParried && turret != null)
        {
            turret.Stun();
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator Kill(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            Destroy(gameObject);
        }
    }
}
