using nobnak.Gist;
using nobnak.Gist.Extensions.Behaviour;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.ColorGradientSystem {

    [ExecuteInEditMode]
    public class ColorGradient : MonoBehaviour {

    public const string PROP_MAIN_TEX = "_MainTex";
        [SerializeField] protected Shader gradientShader;
        [SerializeField] protected Gradation gradation;

        protected Validator validator = new Validator();
        protected ScopedObject<Material> gradientMat;

        private void OnEnable() {
            gradientMat = new ScopedObject<Material>(new Material(gradientShader));

            validator.Validation += () => {
                if (gradation != null)
                    gradation.Invalidate();
            };
        }
        private void OnDisable() {
            validator.Reset();
            gradientMat.Dispose();
        }
        private void OnValidate() {
            validator.Invalidate();
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            validator.CheckValidation();

            var rtdesc = new RenderTextureDescriptor(
                source.width, source.height, source.format, 0);
            rtdesc.sRGB = source.sRGB;

            var src = RenderTexture.GetTemporary(rtdesc);
            RenderTexture dst;
            Graphics.Blit(source, src);

            gradientMat.Data.shaderKeywords = null;

            var aspect = (float)source.width / source.height;
            gradation.Aspect = aspect;

            dst = RenderTexture.GetTemporary(rtdesc);

            gradientMat.Data.SetTexture(PROP_MAIN_TEX, source);
            gradation.SetMaterialProperties(gradientMat);
            Graphics.Blit(src, dst, gradientMat);

            RenderTexture.ReleaseTemporary(src);
            src = dst;

            Graphics.Blit(src, destination);
            RenderTexture.ReleaseTemporary(src);
        }
    }
}