using Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorGradientSystem {

    [ExecuteInEditMode]
    public class ColorGradient : MonoBehaviour {
        public enum BlendModeEnum { None = 0, BLEND_MULT, BLEND_SCRN, BLEND_OVRY }

        public const string PROP_COLOR = "_Color";
        public const string PROP_POSS = "_Poss";
        public const string PROP_COLORS = "_Colors";
        public const string PROP_THROTTLES = "_Throttles";

        [SerializeField] protected ColorGradientData data;
        [SerializeField] protected Shader shader;

        ScopedObject<Material> mat;

        #region Unity
        private void OnEnable() {
            mat = new ScopedObject<Material>(new Material(shader));
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            var baseColor = data.baseColor;
            baseColor.a = data.blendAmount;
            var blendMode = (data.blendMode == BlendModeEnum.None ? null : data.blendMode.ToString());
            var colors = ToMatrix(Colors.ToArray());
            var poss = ToMatrix(Poss.ToArray());
            var throttles = ToVector4(Throttles.ToArray());

            mat.Data.shaderKeywords = null;
            mat.Data.EnableKeyword(blendMode);

            mat.Data.SetColor(PROP_COLOR, baseColor);
            mat.Data.SetMatrix(PROP_COLORS, colors);
            mat.Data.SetMatrix(PROP_POSS, poss);
            mat.Data.SetVector(PROP_THROTTLES, throttles);

            Graphics.Blit(source, destination, mat);
        }
        private void OnDisable() {
            mat.Dispose();
        }
        #endregion

        protected Matrix4x4 ToMatrix(params Vector4[] cols) {
            var m = Matrix4x4.zero;
            for (var i = 0; i < 4 && i < cols.Length; i++)
                m.SetColumn(i, cols[i]);
            return m;
        }
        protected Matrix4x4 ToMatrix(params Color[] cols) {
            return ToMatrix(cols.Select(c=>(Vector4)(c.linear)).ToArray());
        }
        protected Vector4 ToVector4(params float[] vs) {
            var v = Vector4.zero;
            for (var i = 0; i < 4 && i < vs.Length; i++)
                v[i] = vs[i];
            return v;
        }

        protected IEnumerable<Color> Colors {
            get { return data.drops.Select(d => d.color); }
        }
        protected IEnumerable<Vector4> Poss {
            get { return data.drops.Select(d => (Vector4)d.center); }
        }        
        protected IEnumerable<float> Throttles {
            get { return data.drops.Select(d => d.throttle); }
        }
    }
}
