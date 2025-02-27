using UnityEngine;
using System.Collections.Generic;


/*
This code was made using these tutorials:
Brackeys - https://www.youtube.com/watch?v=Dn_BUIVdAPg

and some (maybe a lot of) ChatGPT
and some magic by Beranger
*/
public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    public knightCombat knightCombat;
    private List<GameObject> weapons = new List<GameObject>();
    private List<Transform> colliders = new List<Transform>();


    void Start()
    {
        CacheWeapons();
        SelectWeapon();
    }

    void Update()
    {
        int prevWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedWeapon = (selectedWeapon + 1) % weapons.Count;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedWeapon = (selectedWeapon - 1 + weapons.Count) % weapons.Count;
        }

        if (prevWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void CacheWeapons()
    {
        weapons.Clear();
        colliders.Clear();

        foreach (Transform weapon in transform)
        {
            weapons.Add(weapon.gameObject);
            weapon.gameObject.SetActive(false);

            Transform colliderTransform = weapon.Find("HitCollider");
            colliders.Add(colliderTransform);
        }

        if (weapons.Count > 0)
        {
            weapons[selectedWeapon].SetActive(true);
        }
    }

    void SelectWeapon()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            bool isActive = (i == selectedWeapon);
            weapons[i].SetActive(isActive);

            // Enable/Disable the collider Transform if it exists
            if (colliders[i] != null)
            {
                colliders[i].gameObject.SetActive(isActive);
            }
        }
    }
}
