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
        public const string PROP_GRADIENT_TEX = "_GradientTex";
        public const string PROP_NOISE_TEX = "_NoiseTex";

        public const string PROP_UV_GRADIEN_MATRIX = "_GradientMatrix";

        public const string PROP_COLOR = "_Color";
        public const string PROP_GRADIENT_GAIN = "_GradientGain";
        public const string PROP_NOISE_GAIN = "_NoiseGain";
        public const string PROP_BLEND_MODE = "_BlendMode";


        [SerializeField] protected Material gradientMat;
        [SerializeField] protected Material noiseMat;
        [SerializeField] protected Gradation[] gradations;

        [SerializeField] protected Color gradientColor = Color.white;
        [Range(0f, 10f)]
        [SerializeField]
        protected float gradientGain = 1f;
        [Range(0f, 10f)]
        [SerializeField]
        protected float noiseGain;
        [Range(0, 15)]
        [SerializeField]
        protected int blendMode = 0;

        protected Validator validator = new Validator();
        protected ScopedObject<RenderTexture>[] noiseTextures;

        private void OnEnable() {
            validator.Validation += () => {
                foreach (var g in gradations) {
                    if (g != null)
                        g.Invalidate();
                }
            };
        }
        private void OnDisable() {
            validator.Reset();
        }
        private void OnValidate() {
            validator.Invalidate();
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            validator.CheckValidation();

            var rtdesc = new RenderTextureDescriptor(
                source.width, source.height, RenderTextureFormat.ARGBHalf, 0);
            rtdesc.sRGB = false;

            GenerateNoiseTextures(rtdesc);

            var src = RenderTexture.GetTemporary(rtdesc);
            RenderTexture dst;
            Graphics.Blit(source, src);

            var aspect = (float)source.width / source.height;
            for (var i = 0; i < gradations.Length; i++) {
                var g = gradations[i];
                if (g == null)
                    continue;

                g.Aspect = aspect;

                dst = RenderTexture.GetTemporary(rtdesc);

                gradientMat.SetTexture(PROP_MAIN_TEX, source);
                gradientMat.SetTexture(PROP_GRADIENT_TEX, g.GradientTexture);
                gradientMat.SetTexture(PROP_NOISE_TEX, noiseTextures[i]);

                gradientMat.SetMatrix(PROP_UV_GRADIEN_MATRIX, g.UVGradientMatrix);

                gradientMat.SetColor(PROP_COLOR, gradientColor);
                gradientMat.SetFloat(PROP_GRADIENT_GAIN, gradientGain);
                gradientMat.SetFloat(PROP_NOISE_GAIN, noiseGain);
                gradientMat.SetInt(PROP_BLEND_MODE, blendMode);

                Graphics.Blit(src, dst, gradientMat);

                RenderTexture.ReleaseTemporary(src);
                src = dst;
            }

            Graphics.Blit(src, destination);
            RenderTexture.ReleaseTemporary(src);
        }

        private void GenerateNoiseTextures(RenderTextureDescriptor rtdesc) {
            if (noiseTextures == null || noiseTextures.Length != gradations.Length) {
                if (noiseTextures != null)
                    foreach (var ntex in noiseTextures)
                        ntex.Dispose();
                noiseTextures = new ScopedObject<RenderTexture>[gradations.Length];
            }
            for (var i = 0; i < noiseTextures.Length; i++) {
                if (noiseTextures[i] == null)
                    noiseTextures[i] = new ScopedObject<RenderTexture>(
                        new RenderTexture(rtdesc));

                var tex = noiseTextures[i];
                Graphics.Blit(null, tex, noiseMat);
                noiseTextures[i] = new ScopedObject<RenderTexture>(tex);
            }
        }

        [System.Serializable]
        public class Gradation : System.IDisposable {
            public const int TEX_DENSITY = 256;

            [SerializeField] protected Gradient grad;
            [Range(-1f, 1f)]
            [SerializeField]
            protected float offset = 0f;
            [Range(0f, 360f)]
            [SerializeField]
            protected float rotation = 0f;
            [SerializeField]
            protected TextureWrapMode wrapMode = TextureWrapMode.Repeat;

            protected Validator validator = new Validator();
            protected ScopedObject<Texture2D> tex;
            protected Reactive<float> aspect;
            protected Matrix4x4 uvGradientMatrix;

            public Gradation() {
                aspect = new Reactive<float>(1f);

                aspect.Changed += (v) => validator.Invalidate();

                validator.Validation += () => {
                    GenerateGradiantTexture();
                    GenerateUVGradientMatrix();
                };
            }

            public Texture GradientTexture {
                get {
                    validator.CheckValidation();
                    return tex;
                }
            }
            public float Aspect {
                set {
                    aspect.Value = value;
                }
            }
            public Matrix4x4 UVGradientMatrix {
                get { validator.CheckValidation(); return uvGradientMatrix; }
            }

            public void Dispose() {
                if (tex != null && !tex.Disposed) {
                    tex.Dispose();
                }
            }

            public void Invalidate() {
                validator.Invalidate();
            }
            public void GenerateGradiantTexture() {
                if (tex == null) {
                    tex = new ScopedObject<Texture2D>(
                        new Texture2D(1, 1, TextureFormat.RGBAHalf, false, true));
                }

                tex.Data.filterMode = FilterMode.Bilinear;
                tex.Data.wrapMode = wrapMode;
                tex.Data.Resize(TEX_DENSITY, 1);

                var pixels = tex.Data.GetPixels();
                var dx = 1f / (tex.Data.width - 1);
                for (var i = 0; i < tex.Data.width; i++) {
                    var c = grad.Evaluate(i * dx);
                    pixels[i] = c.linear;
                }
                tex.Data.SetPixels(pixels);
                tex.Data.Apply();
            }

            protected void GenerateUVGradientMatrix() {
                uvGradientMatrix = Matrix4x4.Translate(new Vector3(-offset, -offset, 0f))
                    * Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0f))
                    * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation))
                    * Matrix4x4.Scale(new Vector3(1f, 1f / aspect, 1f))
                    * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f));
            }
        }
    }
}