using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{

    public ParticleSystem bombExplosion;
    public LayerMask enemylayer;
    public GameObject shield;
    bool displayGizmo = false;
    float gizmoDisplayStartTime;
    public PlayerManager player;
    private PlayerManager.AbilityState previousAbilityState;

    // Start is called before the first frame update
    void Start()
    {
        shield.SetActive(false);
        player = GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {

        PlayerManager.AbilityState currentAbility = player.abilityState;

        if (currentAbility != previousAbilityState)
        {
            switch (currentAbility)
            {
                case PlayerManager.AbilityState.normal:
                    break;



                case PlayerManager.AbilityState.Bomb:
                    Bomb();
                    break;

                case PlayerManager.AbilityState.Shield:
                    StartCoroutine(Shield());
                    break;


                case PlayerManager.AbilityState.speedUp:
                    StartCoroutine(Speed());

                    break;
            }

            previousAbilityState = currentAbility;
        }


        if (displayGizmo && Time.time - gizmoDisplayStartTime > 1)
        {
            displayGizmo = false;
        }
    }


    void Bomb()
    {
        displayGizmo = true;
        gizmoDisplayStartTime = Time.time;
        Instantiate(bombExplosion, transform.position, Quaternion.identity, transform);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.0f, enemylayer);
        foreach (Collider2D collider in colliders)
        {
            FollowPlayer script = collider.GetComponent<FollowPlayer>();
            if (script != null)
            {
                script.isDestroy = true;
            }

        }

        //ChangeState(Player2.AbilityState.normal);

    }

    private IEnumerator Shield()
    {
        shield.SetActive(true);

        yield return new WaitForSeconds(4.0f);

        for (int i = 20; i > -1; i--)
        {
            shield.GetComponent<SpriteRenderer>().material.SetFloat("_clipTime", 0.05f * i);
            yield return new WaitForSeconds(0.05f);
        }

        shield.SetActive(false);
        shield.GetComponent<SpriteRenderer>().material.SetFloat("_clipTime", 1);
        ChangeState(PlayerManager.AbilityState.normal);

    }

    private IEnumerator Speed()
    {
        player.forceMagnitude *= 5f;

        yield return new WaitForSeconds(5.0f);

        player.forceMagnitude /= 5f;

        ChangeState(PlayerManager.AbilityState.normal);

    }

    private GameObject findClosestEnemy()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        // 最も近い敵を検索
        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }



    private void OnDrawGizmos()
    {
        if (displayGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2);
        }

    }

    private void ChangeState(PlayerManager.AbilityState newState)
    {
        player.abilityState = newState;

        Debug.Log("Player State Changed to: " + newState);
    }



}
