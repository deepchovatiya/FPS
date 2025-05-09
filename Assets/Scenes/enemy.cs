
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class enemy : MonoBehaviour
{
    public float health = 3;
    //private player playerScript;
    public Transform player;
    public float chaseRange = 5f;
    public float attackRange = 1.5f; // Changed from 200f to 1.5f
    public float attackCooldown = 2f;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private Animator animator;
    public GameObject win;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        //playerScript = player.GetComponent<player>();
        win.SetActive(false);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            agent.SetDestination(player.position);
            animator.SetBool("run", agent.velocity.magnitude > 0.1f);

            if (distance <= attackRange)
            {
                TryAttack();
            }
        }
        else
        {
            agent.ResetPath(); // Stop moving
            animator.SetBool("run", false);
        }
    }

    void TryAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            // Face the player
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            if (animator != null)
            {
                animator.SetTrigger("attack");
                Debug.Log("Enemy attack triggered!");

            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //if (playerScript != null)
            //{
            //    playerScript.TakeDamage(1f); // Damage can be adjusted
            //}
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        animator.SetTrigger("Die");
        win.SetActive(true);
        StartCoroutine(WaitAndDestroy());
    }

    IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(2f); // Wait for the animation to finish
        Destroy(gameObject);
    }

    public void btnnext()
    {
        win.SetActive(false);
        SceneManager.LoadScene("FirstPerson");
    }
}