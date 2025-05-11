using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FollowPlayer : MonoBehaviour
{
    private Camera mainCamera;

    public float followDistance = 3f;

    NavMeshAgent2D agent;


    Rigidbody2D rb;
    GameObject target;
    EnemyManager enemyManager;
    GameManager gameManager;
    public ParticleSystem particle,punk;
    public LayerMask enemylayer;

    public enum EnemyType
    {
        normal, timeExtender, highScore, punk, speedUp
    }

    private Vector3 lastPosition;
    public EnemyType enemyType = EnemyType.normal;


    public bool isDestroy = false;


    Vector2 A, C, AB, AC;
    Vector2 move = new Vector2(1, 0); // 進む方向
    float speed = 1f; // 動くスピード
    float arot = 0; // 自分の角度

    float Maxkaku = 0.05f; // 曲がる最大角度
    public float rotation; // 曲がる角度

    public delegate void EnemyDestroyed(EnemyType enemyType);
    public static event EnemyDestroyed OnDestroyed;

    void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed(enemyType);
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent2D>();
        target = GameObject.FindGameObjectWithTag("Player");
        enemyManager = FindObjectOfType<EnemyManager>();
        gameManager = FindObjectOfType<GameManager>();

        lastPosition = transform.position;
        rb = gameObject.GetComponent<Rigidbody2D>();


    }

    void Update()
    {
        if (target != null)
        {
            //距離の計算
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
            if (distanceToTarget > followDistance)
            {
                transform.position = Utility.CustomFunctions.GetRandomNavMeshPositionOutsideCamera(0.5f);
            }


            runawayPlayer();

            direction();



        }
        else return;


        if (isDestroy)
        {
            Deth();
        }




    }



    //当たり判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (target != null)
        {
            bool fast = target.GetComponent<PlayerManager>().isMovingFast;


            if (collision.gameObject.tag == "Player")
            {
                if (enemyType != EnemyType.timeExtender)
                {
                    if (fast)
                    {
                        Deth();

                    }
                }
                else
                {
                    Deth();
                }


            }
        }

    }


    void Deth()
    {
        List<GameObject> enemies = enemyManager.GetComponent<EnemyManager>().ENEMIES;



        switch (enemyType)
        {
            case EnemyType.normal:
                Destroy(gameObject);
                enemies.Remove(gameObject);
                Instantiate(particle, transform.position, Quaternion.identity);

                break;

            case EnemyType.timeExtender:

                Destroy(gameObject);
                enemies.Remove(gameObject);
                Instantiate(particle, transform.position, Quaternion.identity);

                break;

            case EnemyType.highScore:

                break;

            case EnemyType.punk:

                StartCoroutine(DeathAnimation());
                break;

            case EnemyType.speedUp:

                break;


        }

        IEnumerator DeathAnimation()
        {
            Animator anim = GetComponent<Animator>();
            anim.SetBool("death", true);

            // アニメーションの終了を待つ
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5f, enemylayer);

            foreach (Collider2D collider in colliders)
            {
                FollowPlayer script = collider.GetComponent<FollowPlayer>();
                if (script != null)
                {
                    script.isDestroy = true;
                }
            }

            Destroy(gameObject);
            enemies.Remove(gameObject); // enemies リストからの削除は適切なタイミングで行う必要があります
            Instantiate(particle, transform.position, Quaternion.identity);
            Instantiate(punk, transform.position, Quaternion.identity);
        }
    }



    void direction()
    {
        //向きの計算
        Vector2 direction = (target.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        Vector3 currentPosition = transform.position;
        Vector3 movementDirection = currentPosition - lastPosition;

        // 移動している方向が右ならスケールを(1, 1, 1)に、左なら(-1, 1, 1)に設定
        if (movementDirection.x > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (movementDirection.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        lastPosition = currentPosition;
    }

    void runawayPlayer()
    {
        A = new Vector2(transform.position.x, transform.position.y); // 自分のポジション
        C = new Vector2(target.transform.position.x, target.transform.position.y); // ターゲットポジション

        AB = new Vector2(move.x, move.y); // 移動方向

        AC = C - A; // ターゲットのベクトルを調べる

        //なす角ｒを求める
        float dot = AB.x * AC.x + AB.y * AC.y; // 内積

        float r = Acosf(dot / ((float)length(AB) * (float)length(AC))); // アークコサインを使って内積とベクトルの長さから角度を求める

        // 曲がる方向を決める
        if (AB.x * AC.y - AB.y * AC.x < 0)
        {
            r = -r;
        }

        r = r * 180 / Mathf.PI; // ラジアンから角度に

        // 回転角度制御
        if (r > Maxkaku)
        {
            r = Maxkaku;
        }
        if (r < -Maxkaku)
        {
            r = -Maxkaku;
        }


        rotation = r; // 曲がる角度を入れる


        Move();
    }

    void Move()
    {
        float rot = rotation; // 曲がる角度

        float tx = move.x, ty = move.y;

        move.x = tx * Mathf.Cos(rot) - ty * Mathf.Sin(rot);
        move.y = tx * Mathf.Sin(rot) + ty * Mathf.Cos(rot);

        arot = Mathf.Atan2(move.x, move.y); // 移動量から角度を求める
        float kaku = arot * 180.0f / Mathf.PI * -1 + 90; // ラジアンから角度に

        rb.velocity = new Vector2(move.x, move.y) * speed * -1; // 移動(最後のー1をかけている所を消すとプレイヤーを追いかけます)
        transform.rotation = Quaternion.Euler(0, 0, kaku); // 回転

    }
    /// <summary>
    /// 長さが+-1を越えたとき1に戻す処理
    /// </summary>
    /// <param name="a">内積 / ベクトルの長さの答</param>
    /// <returns></returns>
    public float Acosf(float a)
    {
        if (a < -1) a = -1;
        if (a > 1) a = 1;

        return (float)Mathf.Acos(a);
    }

    /// <summary>
    /// ベクトルの長さを求める
    /// </summary>
    /// <param name="vec">2点間のベクトル</param>
    /// <returns>ベクトルの長さを返す</returns>
    public float length(Vector2 vec)
    {
        return Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y);
    }

}
