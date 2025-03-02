using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This code was made using these tutorials:
Brackeys - (MELEE COMBAT in Unity) https://www.youtube.com/watch?v=sPiVz1k-fEs

and some ChatGPT
and some magic by Beranger
*/

public class knightCombat : MonoBehaviour
{
    [Header("References")]
    public knightController knightController;

    [Header("Combat")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public Transform attackPoint; // This has a CapsuleCollider
    public LayerMask enemyLayers;
    private bool isAttacking = false; // Prevents multiple hits per frame
    private HashSet<Collider> hitEnemies = new HashSet<Collider>();
    public int damage;
    public float delayBetweenAttacks = 0.5f;


    private void Update() 
    {
        if (knightController.canMove && knightController.grounded && Input.GetKeyDown(attackKey) && !isAttacking)
        {
            Attack();
        }
    }

    public void Attack()
    {   
        // Enable the collider briefly
        hitEnemies.Clear();
        isAttacking = true;
        // Play attack animation
        knightController.ChangeAnimation(knightController.Attack1);
        
        Invoke(nameof(DisableAttack), 1.25f + delayBetweenAttacks); // Adjust time based on animation
    }

    private void DisableAttack()
    {
        isAttacking = false;
        hitEnemies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Weapon in range");
        if (isAttacking && ((1 << other.gameObject.layer) & enemyLayers) != 0)
        {
            
            if (!hitEnemies.Contains(other)) // Prevent multiple hits on the same enemy
            {
                hitEnemies.Add(other); // Mark this enemy as hit
                //Debug.Log(other.name + " hit!");
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    Debug.Log("knight doing damage");
                    enemy.TakeDamage(damage); // Example damage value
                }
            }
        }
    }

    /*
        private void OnDrawGizmosSelected() 
        {
            if(attackPoint == null)
            {
                Debug.Log("No attack point set");
                return;
            }
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    */
}
