using nobnak.Gist;
using nobnak.Gist.Scoped;
using UnityEngine;
using nobnak.Gist.Extensions.Behaviour;

namespace nobnak.ColorGradientSystem {

    [CreateAssetMenu(fileName = "Gradation", menuName = "ColorGradient/Gradation")]
    public class Gradation : ScriptableObject, System.IDisposable {
        public const int TEX_DENSITY = 256;
        public const float SQRT_OF_TWO = 1.41421356237f;

        public const string PROP_GRADIENT_TEX = "_GradientTex";
        public const string PROP_GRADIENT_OPACITY = "_GradientOpacity";
        public const string PROP_GRADIENT_BLEND_MODE = "_GradientBlendMode";
        public const string PROP_UV_GRADIEN_MATRIX = "_GradientMatrix";

        public enum BlendModeEnum {
            blend_normal = 0,
            blend_darken,
            blend_multiply,
            blend_color_burn,
            blend_lienar_burn,
            blend_lighten,
            blend_screen,
            blend_color_dodge,
            blend_linear_dodge,
            blend_overlay,
            blend_soft_light,
            blend_hard_light,
            blend_vivid_light,
            blend_linear_light,
            blend_pin_light,
            blend_difference,
            blend_exclusion
        }

        [SerializeField] protected Gradient grad = new Gradient();
        [Range(0f, 360f)]
        [SerializeField]
        protected float rotation = 0f;
        [Range(0.1f, 1.5f)]
        [SerializeField]
        protected float ratio = 1f;
        [SerializeField]
        protected TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        [Range(0f, 1f)]
        [SerializeField]
        protected float opacity = 1f;
        [SerializeField]
        protected BlendModeEnum blendMode = BlendModeEnum.blend_overlay;

        protected Validator validator = new Validator();
        protected ScopedObject<Texture2D> tex;
        protected Reactive<float> aspect;
        protected Matrix4x4 uvGradientMatrix;

        #region Unity
        protected void OnEnable() {
            aspect = new Reactive<float>(1f);
            tex = new ScopedObject<Texture2D>(null);

            aspect.Changed += (v) => validator.Invalidate();

            validator.Reset();
            validator.Validation += () => {
                GenerateGradiantTexture();
                GenerateUVGradientMatrix();
            };
        }
        protected void OnValidate() {
            validator.Invalidate();
        }
        #endregion

        public Texture GradientTexture {
            get {
                validator.Validate();
                return tex;
            }
        }
        public float Aspect {
            set {
                aspect.Value = value;
            }
        }
        public Matrix4x4 UVGradientMatrix {
            get { validator.Validate(); return uvGradientMatrix; }
        }

        public void Dispose() {
            if (tex != null && !tex.Disposed) {
                tex.Dispose();
            }
        }

        public void SetMaterialProperties(Material gradientMat) {
            gradientMat.SetInt(PROP_GRADIENT_BLEND_MODE, (int)blendMode);
            gradientMat.SetTexture(PROP_GRADIENT_TEX, GradientTexture);
            gradientMat.SetFloat(PROP_GRADIENT_OPACITY, opacity);
            gradientMat.SetMatrix(PROP_UV_GRADIEN_MATRIX, UVGradientMatrix);
        }
        public void Invalidate() {
            validator.Invalidate();
        }
        public void GenerateGradiantTexture() {
            if (tex == null || tex.Disposed)
                return;

            var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            if (tex.Data == null)
                tex.Data = new Texture2D(1, 1, TextureFormat.RGBAHalf, false, linear);

            tex.Data.filterMode = FilterMode.Bilinear;
            tex.Data.wrapMode = wrapMode;
            tex.Data.Resize(TEX_DENSITY, 1);

            var pixels = tex.Data.GetPixels();
            var dx = 1f / (tex.Data.width - 1);
            for (var i = 0; i < tex.Data.width; i++) {
                var c = grad.Evaluate(i * dx);
                pixels[i] = (linear ? c.linear : c);
            }
            tex.Data.SetPixels(pixels);
            tex.Data.Apply();
        }

        protected void GenerateUVGradientMatrix() {
            var scale = 1f / (ratio * SQRT_OF_TWO);

            uvGradientMatrix = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0f))
                * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -rotation))
                * Matrix4x4.Scale(new Vector3(scale, scale, 1f))
                * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f));
        }
    }
}
