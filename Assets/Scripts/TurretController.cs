using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] Transform bulletParent;
    [SerializeField] GameObject stunOverlay;
    [SerializeField] float fireTime = 3f;

    private float fireTimer = 0f;
    private bool isStunned = false;
    private float stunTime = 3f;
    private float stunTimer = 0f;

    // Sounds
    [SerializeField] AudioSource fireSound;
    [SerializeField] AudioSource turretHitSound;

    private void Update()
    {
        if (!isStunned)
        {
            fireTimer += Time.deltaTime;

            // Check if should fire
            if (fireTimer >= fireTime)
            {
                Fire();

                // Reset timer
                fireTimer = 0f;
            }
        }
        else // If stunned
        {
            stunTimer += Time.deltaTime;

            // Check if stun is over
            if (stunTimer >= stunTime)
            {
                isStunned = false;
                stunTimer = 0f;
                stunOverlay.SetActive(false);
            }
        }
    }

    private void Fire()
    {
        Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity, bulletParent);
        fireSound.Play();
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public void Stun()
    {
        isStunned = true;
        stunOverlay.SetActive(true);
        turretHitSound.Play();
    }
}
