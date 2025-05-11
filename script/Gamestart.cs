using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Gamestart : MonoBehaviour
{
    public float overlapRadius = 1f; 
    public float overlapDuration = 3f; 
    private float overlapTimer = 0f;
    private bool isOverlapping = false;
    public GameObject circle;
    public Animator button;

    public LayerMask playerLayer; 

    private void Update()
    {
      
        Vector2 objectPosition = transform.position;
  
        Collider2D hit = Physics2D.OverlapCircle(objectPosition, overlapRadius, playerLayer);

        if (hit != null)
        {
            // 衝突したオブジェクトがプレイヤーであるか確認
            if (hit.CompareTag("Player"))
            {
                isOverlapping = true;
                button.SetBool("on", true);
            }
            else
            {
                isOverlapping = false;
                button.SetBool("on", false);
                overlapTimer = 0f; // 重なりから離れたらタイマーをリセット
            }
        }
        else
        {
            isOverlapping = false;
            button.SetBool("on", false);
            overlapTimer = 0f; // 何にも当たらなければタイマーをリセット
        }

        circle.GetComponent<SpriteRenderer>().material.SetFloat("_Float", Utility.CustomFunctions.Map(overlapTimer, 0, 3f, 1, 0));

        // 重なっている場合はタイマーを増加
        if (isOverlapping)
        {
            overlapTimer += Time.deltaTime;
            

            if (overlapTimer >= overlapDuration)
            {
                Debug.Log("Player has been overlapping for " + overlapDuration + " seconds.");

                SceneManager.LoadScene("Slingshot");
            }
        }
    }
}
