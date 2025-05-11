using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPlayController : MonoBehaviour
{
    public Transform target1; // ターゲット1
    public Transform target2; // ターゲット2
    public float smoothSpeed = 0.125f; // カメラの追従のスムーズさ
    public float minZoom = 5f; // 最小ズーム値
    public float maxZoom = 15f; // 最大ズーム値
    public float zoomSpeed = 2f; // ズーム速度

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // ターゲットの中心点を計算
        Vector3 centerPoint = (target1.position + target2.position) / 2f;

        // ターゲット間の距離に基づいてカメラのズームレベルを計算
        float targetDistance = Vector3.Distance(target1.position, target2.position);
        float targetZoom = Mathf.Clamp(targetDistance, minZoom, maxZoom);

        // カメラの位置とズームを更新
        Vector3 newPosition = centerPoint;
        newPosition.z = -targetZoom;
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);

        // カメラの向きを更新
        transform.LookAt(centerPoint);

        // マウスホイールでズームイン・ズームアウト
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollWheel * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }
}
