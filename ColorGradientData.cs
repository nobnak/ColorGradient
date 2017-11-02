using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGradientSystem {

    [CreateAssetMenu]
    public class ColorGradientData : ScriptableObject {

        public ColorGradient.BlendModeEnum blendMode;
        [ColorUsage(false, true, 0f, 8f, 1f / 8, 3f)] public Color baseColor = Color.white;
        [Range(0f, 1f)] public float blendAmount = 1f;
        public Drop[] drops;

        [System.Serializable]
        public class Drop {
            public Color color;
            public Vector2 center;
            [Range(0f, 1f)] public float throttle;

            public Drop() {
                this.color = Color.white;
                this.throttle = 1f;
            }
        }
    }
}
