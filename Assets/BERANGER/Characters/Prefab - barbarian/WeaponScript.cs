using UnityEngine;
using System.Collections.Generic;

public class WeaponScript : MonoBehaviour
{
    public int damage; // Set this per weapon
    public LayerMask playerLayer;
    private bool canDealDamage = false; // Control when it can deal damage
    private HashSet<Collider> hitPlayers = new HashSet<Collider>();

    public void ActivateWeapon() // Called by enemy when attacking
    {
        canDealDamage = true;
        hitPlayers.Clear();
    }

    public void DeactivateWeapon() // Called after attack finishes
    {
        canDealDamage = false;
        hitPlayers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canDealDamage && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if(!hitPlayers.Contains(other))
            {
                hitPlayers.Add(other);
                knightController player = other.GetComponent<knightController>();
                if (player != null)
                {
                    Debug.Log("Weapon hit: " + player.name);
                    player.TakeDamage(damage);
                }
            }
            
        }
    }
}

