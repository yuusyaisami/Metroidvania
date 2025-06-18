using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public LayerMask waterMask;
    public float rayLength = 5f;
    public float minImpactVelocity = 1.5f;
    public AnimationCurve velocityToForce;

    private Rigidbody2D rb;
    private bool wasInWaterLastFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, waterMask);
        bool isInWaterNow = hit.collider != null;

        if (isInWaterNow && !wasInWaterLastFrame)
        {
            WaterSurface surface = hit.collider.GetComponent<WaterSurface>();
            if (surface != null)
            {
                Vector3 splashPos = surface.ClosestSurfacePoint(hit.point);

                // 🟡 波形による水面のY座標を取得
                float surfaceY = splashPos.y;

                // 🟡 Rayのヒット位置より水面が上なら = "接触してない"
                if (hit.point.y > surfaceY)
                {
                    // 描画された水面の下には達していない
                    return;
                }

                // ✅ 実際に波に触れたとみなせる場合だけSplash
                if (rb.velocity.y < -minImpactVelocity)
                {
                    float rawVelocity = Mathf.Max(-rb.velocity.y, 0f); // 正の値に変換（落下速度）

                    // y = -5 → 0.2, y = -10 → 0.8 にマッピング
                    float force = Mathf.Clamp01((rawVelocity - 5f) / (10f - 5f)); // [0,1]に正規化
                    force = Mathf.Lerp(0f, 0.8f, force); // 最終force値

                    surface.Splash(splashPos, force);



                    Debug.Log($"Splash at {splashPos} with velocity {rb.velocity.y}, force {force}");
                    surface.Splash(splashPos, force);
                }
            }
        }



        wasInWaterLastFrame = isInWaterNow;
    }
}
