using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{

    public bool isMovingFast = false;
    public bool isStarted;
    bool isCharged = false;

    public Rigidbody2D rb;


    public LayerMask layermask;
    public AudioSource audioSource;

    public AudioClip reflect;
    public AudioClip turboBoost;
    public ParticleSystem charged;


    public float forceMagnitude = 10.0f;
    public float movespeed = 1.0f;
    public float inputThreshold = 2.0f;
    public float moveSpeedThreshold = 5.0f;


    public float duration = 1;
    private float previousValue;

    public float currentHoldTime = 0;

    public float normalizedValue;

    public enum AbilityState
    {
        normal, Bomb, Shield, speedUp
    }

    public AbilityState abilityState = AbilityState.normal;

    public delegate void PlayerGroundedEventHandler();
    public static event PlayerGroundedEventHandler OnPlayerGrounded;

    void Start()
    {
        isStarted = true;

    }

    private void FixedUpdate()
    {
        if (isStarted)
        {

            //入力速度のチェック
            float currentValue = normalizedValue;
            if (previousValue > currentValue)
            {
                float changeSpeed = Mathf.Abs(currentValue - previousValue) / Time.deltaTime;


                //変化速度が閾値より大きいかつ一定以上引っ張られた時
                if (changeSpeed > inputThreshold && !isMovingFast)

                {

                    Vector2 forwardDirection = transform.up;
                    Vector2 force = forwardDirection * forceMagnitude * (previousValue * 0.5f);
                    rb.AddForce(force, ForceMode2D.Impulse);

                    audioSource.PlayOneShot(turboBoost);
                    isCharged = false;



                    currentHoldTime = 0;

                }
            }
            previousValue = currentValue;
        }
    }

    void Update()
    {
        if (isStarted)
        {


            //入力設定
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = -Input.GetAxis("Vertical");

            if (horizontalInput != 0.0f || verticalInput != 0.0f)
            {
                float angle = Mathf.Atan2(verticalInput, horizontalInput) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
                if (horizontalInput > 0)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }

                rb.AddForce(transform.up * movespeed);
            }


            float currentAxisValue = Input.GetAxis("Pull");
            if (currentAxisValue != 0)
            {
                if (Mathf.Abs(currentAxisValue) > 0)
                {

                    float originalValue = Input.GetAxis("Pull");

                    normalizedValue = Utility.CustomFunctions.Map(originalValue, -1, 1, 0, 3);



                    //移動速度のチェック
                    if (rb.velocity.magnitude > moveSpeedThreshold)
                    {
                        isMovingFast = true;

                    }
                    else
                    {
                        isMovingFast = false;
                    }

                    //入力強度のチェック

                    if (normalizedValue == 3)
                    {
                        currentHoldTime += Time.deltaTime;
                    }

                    if (normalizedValue == 0 && !isCharged)
                    {
                        //currentHoldTime = 0;
                    }

                    if (currentHoldTime > duration && !isCharged)
                    {
                        isCharged = true;
                        charged.Play();
                    }

                    //急停止

                    if (Input.GetButton("joystick button 1"))
                    {
                        rb.velocity *= 0.9f;
                        //Time.timeScale = 0.2f;
                    }
                    if (Input.GetButtonUp("joystick button 1"))
                    {
                        //Time.timeScale = 1;
                    }







                }
            }

        }
    }

    //当たり判定
    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (collision.gameObject.tag == "Ground")
        {
            audioSource.PlayOneShot(reflect);
            OnPlayerGrounded?.Invoke();
        }
    }



}
