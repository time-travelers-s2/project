using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
This code was made using these tutorials:
Dave / GameDevelopment - https://www.youtube.com/watch?v=UjkSFoLxesw



and some magic by Beranger
*/


public class EnemyController : MonoBehaviour, Character
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    private NavMeshPath mPath;

    [Header("Movement")]
    public float walkSpeed;
    public float RunSpeed;

    [Header("Patroling")]
    public Transform patrolPoint;
    private bool canPatrol;
    private Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;
    private bool isWaiting = false;

    [Header("Combat")]
    public float cooldownToAttack;
    private float cooldownToAttackTimer = 0;
    public int damage;
    public LayerMask playerLayers;
    private bool isAttacking = false; // Prevents multiple hits per frame
    public WeaponScript weapon;


    // Distances
    [Header("Distances")]
    public float sightRange;
    public float attackRange;
    private float toPlayerDistance;

    // Stats
    [Header("Health")]
    public int maxHealth;
    private int currentHealth;


    //Animations
    private Animator animator;
    private string currentAnimation;

    [Header("Animations")]
    public string Idle = "Idle";
    public string Running = "Running_A";
    public string Walking = "Walking_A";
    public string Attacking = "1H_Melee_Attack_Slice_Diagonal";
    public string Hit = "Hit_A";
    public string Death = "Death_A";

    //Other



    void Start()
    {
        currentHealth = maxHealth;

        weapon.damage = damage;
        weapon.playerLayer = playerLayers;

        agent = GetComponent<NavMeshAgent>();
        mPath = new NavMeshPath();

        animator = GetComponent<Animator>();
        ChangeAnimation(Idle);
        
        canPatrol = patrolPoint != null;
        if(canPatrol)
        {
            Debug.Log(patrolPoint.position);
            StartCoroutine(StartPatrolWithDelay(2f)); // Wait for 5 seconds before starting patrol
        }
        Debug.Log(currentHealth);

    }

    IEnumerator StartPatrolWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        //Debug.Log("Starting patrol...");
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
    }

    private void FixedUpdate()
    {
        toPlayerDistance = CalculatePathDistance(player.position);
        if(toPlayerDistance == -1 || isAttacking) return;
        if(toPlayerDistance < attackRange )
        {
            //attack
            isWaiting = false;
            transform.LookAt(player);
            if(!isAttacking && cooldownToAttackTimer <= 0) Attack();
        }
        else if (toPlayerDistance < sightRange)
        {
            //chase player
            agent.speed = RunSpeed;
            StopAllCoroutines();
            Chase();
            isWaiting = false;
        }
        else
        {
            // Idle around || Patrol
            agent.speed = walkSpeed;
            if(canPatrol) Patroling();
            else ChangeAnimation(Idle);
        }
        if (cooldownToAttackTimer > 0)
        {
            cooldownToAttackTimer -= Time.fixedDeltaTime; 
        }
    }





    ////////////////////////
    // Patrol
    private void Patroling()
    {
        if(!walkPointSet) 
        {
            SearchWalkPoint();
        }
        if(walkPointSet && !isWaiting) 
        {
            ChangeAnimation(Walking);
            agent.SetDestination(walkPoint);
        
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f && !isWaiting) 
            {
                // Start waiting at the walk point
                ChangeAnimation(Idle);
                StartCoroutine(WaitAtWalkPoint(5f)); // Wait for 5 seconds
            }
            
        }
    }
    private IEnumerator WaitAtWalkPoint(float waitTime)
    {
        isWaiting = true; // Set the waiting flag
        agent.isStopped = true; // Stop the NavMeshAgent movement
        agent.ResetPath();

        yield return new WaitForSeconds(waitTime); // Wait for the specified time

        agent.isStopped = false; // Resume the NavMeshAgent movement
        walkPointSet = false; // Allow the enemy to pick a new walk point
        isWaiting = false; // Reset the waiting flag
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        
        //Gets a random point around the patrolPoint
        Vector3 potentialWalkPoint = new Vector3(patrolPoint.position.x + randomX, patrolPoint.position.y, patrolPoint.position.z + randomZ);
        // Align the y value with the NavMesh or terrain height
        if (NavMesh.SamplePosition(potentialWalkPoint, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas))
        {
            
            if (CalculatePathDistance(walkPoint) != -1)
            {
                // Set the walk point to the valid position on the NavMesh
                walkPoint = hit.position; 
                walkPointSet = true;
            }
                
        }
    }


    ////////////////////////
    // Chasing
    private float CalculatePathDistance(Vector3 targetPosition)
    {
        // Calculate a path to the target position
        if (agent.CalculatePath(targetPosition, mPath))
        {
            // Add up the distances between all points in the path
            float totalDistance = 0.0f;
            Vector3[] corners = mPath.corners;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(corners[i], corners[i + 1]);
            }

            return totalDistance;
        }

        // Return -1 if the path cannot be calculated (when target is unreachable)
        return -1;
    }

    private void Chase()
    {
        ChangeAnimation(Running);
        walkPoint = player.position;
        agent.SetDestination(player.position);
    }


    ////////////////////////
    // Attack
    
    public void Attack()
    {   
        agent.isStopped = true; 
        transform.LookAt(player);
        
        isAttacking = true;
        ChangeAnimation(Attacking);

        if (weapon != null)
        {
            weapon.ActivateWeapon(); // Enable weapon collider
        }

        Invoke(nameof(EndAttack), 1.25f); // Adjust timing based on animation
    }

    private void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
        cooldownToAttackTimer = cooldownToAttack;
        if (weapon != null)
        {
            weapon.DeactivateWeapon(); // Disable weapon collider
        }
    }

    public void TakeDamage(int damage)
    {
        ChangeAnimation(Hit);
        currentHealth -= damage;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            Dead();
        }
    }
    private void Dead()
    {
        Destroy(gameObject);    
    }




    ////////////////////////
    // Animations
    private void CheckAnimation()
    {
        if(currentAnimation == Walking || currentAnimation == Running || currentAnimation == Hit)
            return;
        
        ChangeAnimation(Idle);
    }

    public void ChangeAnimation(string animation, float crossfade = 0.2f, float time = 0f)
    {
        if(time > 0 )
            StartCoroutine(Wait());
        else
            Validate();
        
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time - crossfade);
            Validate();
        }


        void Validate()
        {
            if(currentAnimation != animation)
            {
                currentAnimation = animation;
                if(currentAnimation == "")
                    CheckAnimation();
                else
                    animator.CrossFade(animation, crossfade);
            }
        }
        
    }
}
