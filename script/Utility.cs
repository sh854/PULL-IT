using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Utility
{
    public class Physics2DUtil
    {
        public static List<Vector2> ReflectionLinePoses(Vector2 position, Vector2 direction, float length, LayerMask layerMask)
        {
            var points = new List<Vector2>() { position };
            int maxReflections = 10;

            for (int i = 0; i < maxReflections; i++)
            {
                var hit = Physics2D.Raycast(position, direction, length, layerMask);
                if (hit)
                {
                    position = hit.point;
                    points.Add(position);

                    length -= hit.distance;
                    direction = Vector2.Reflect(direction, hit.normal);
                    hit = Physics2D.Raycast(position, direction, length, layerMask);
                }
                else
                {
                    break;
                }
            }

            points.Add(position + direction * length);
            return points;
        }

    }


    public class CustomFunctions : MonoBehaviour
    {
        public static Vector3 GetRandomNavMeshPositionOutsideCamera(float range)
        {
            Vector3 randomPosition = Vector3.zero;
            NavMeshHit hit;
            Camera mainCamera = Camera.main;

            while (true)
            {
                Vector3[] randomPoints = new Vector3[4];

                for (int i = 0; i < 4; i++)
                {
                    Vector2 min = Vector2.zero;
                    Vector2 max = Vector2.zero;

                    switch (i)
                    {
                        case 0:
                            min = mainCamera.ViewportToWorldPoint(new Vector2(-range, -range));
                            max = mainCamera.ViewportToWorldPoint(new Vector2(0, 1 + range));
                            break;
                        case 1:
                            min = mainCamera.ViewportToWorldPoint(new Vector2(1, -range));
                            max = mainCamera.ViewportToWorldPoint(new Vector2(1 + range, 1 + range));
                            break;
                        case 2:
                            min = mainCamera.ViewportToWorldPoint(new Vector2(0, -range));
                            max = mainCamera.ViewportToWorldPoint(new Vector2(1, 0));
                            break;
                        case 3:
                            min = mainCamera.ViewportToWorldPoint(new Vector2(0, 1));
                            max = mainCamera.ViewportToWorldPoint(new Vector2(1, 1 + range));
                            break;
                    }

                    float x = Random.Range(min.x, max.x);
                    float y = Random.Range(min.y, max.y);

                    randomPoints[i] = new Vector3(x, y, 0);
                }

                //randomPosition = randomPoints[Random.Range(0, 4)];

                if (NavMesh.SamplePosition(randomPoints[Random.Range(0, 4)], out hit, 10.0f, NavMesh.AllAreas))
                {
                    randomPosition = hit.position;
                    break;
                }
            }

            return randomPosition;
        }

        public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static IEnumerator DoShake(Transform transform, float duration, float magnitude)
        {
            var pos = transform.localPosition;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var x = pos.x + Random.Range(-1f, 1f) * magnitude;
                var y = pos.y + Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(x, y, pos.z);

                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = pos;
        }

        public static IEnumerator DoShakeRect(RectTransform rectTransform, float duration, float magnitude)
        {
            var pos = rectTransform.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var x = pos.x + Random.Range(-1f, 1f) * magnitude;
                var y = pos.y + Random.Range(-1f, 1f) * magnitude;

                rectTransform.anchoredPosition = new Vector2(x, y);

                elapsed += Time.deltaTime;

                yield return null;
            }

            rectTransform.anchoredPosition = pos;
        }

        public static IEnumerator DelayResetState(float delay, PlayerManager player)
        {

            yield return new WaitForSeconds(delay);

            player.abilityState = PlayerManager.AbilityState.normal;

        }

        private static float elapsedTime = 0.0f;
        private static float previousValue;

        public static bool ValueNotChanged(float value, float duration)
        {
            if (value != previousValue)
            {
                previousValue = value;
                elapsedTime = 0.0f;
            }

            elapsedTime += Time.deltaTime;

            return elapsedTime >= duration;
        }

    }



}

