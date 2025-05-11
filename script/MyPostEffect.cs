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

   
    class GrayScalePass : ScriptableRenderPass
    {
        private readonly string profilerTag = "GrayScale Pass";

        public Material postEffectMaterial; 

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

         
            var cmd = CommandBufferPool.Get(profilerTag);

            cmd.Blit(cameraColorTarget, cameraColorTarget, postEffectMaterial);

            context.ExecuteCommandBuffer(cmd);
        }
    }
}
