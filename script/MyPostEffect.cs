using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostEffect : ScriptableRendererFeature
{
    [SerializeField] private GrayscaleSetting settings = new GrayscaleSetting();
    private GrayScalePass scriptablePass;

    public override void Create()
    {
        if (settings.material != null)
        {
            scriptablePass = new GrayScalePass();
            scriptablePass.postEffectMaterial = settings.material;
            scriptablePass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (scriptablePass != null && scriptablePass.postEffectMaterial != null)
        {
            // レンダーキューに登録 (ポストエフェクト実行)
            renderer.EnqueuePass(scriptablePass);
        }
    }


    [System.Serializable]
    public class GrayscaleSetting
    {
        // ポストエフェクトに使用するマテリアル
        public Material material;

        // レンダリングの実行タイミング
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    /// <summary>
    /// Grayscale実行Pass
    /// </summary>
    class GrayScalePass : ScriptableRenderPass
    {
        private readonly string profilerTag = "GrayScale Pass";

        public Material postEffectMaterial; // グレースケール計算用マテリアル

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            // コマンドバッファ
            var cmd = CommandBufferPool.Get(profilerTag);

            // マテリアル実行
            cmd.Blit(cameraColorTarget, cameraColorTarget, postEffectMaterial);

            context.ExecuteCommandBuffer(cmd);
        }
    }
}