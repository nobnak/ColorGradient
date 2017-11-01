using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGradientSystem {

    [System.Serializable]
    public class ColorGradientData {
        public ColorGradient.BlendModeEnum blendMode;
        [ColorUsage(false, true, 0f, 8f, 1f / 8, 3f)] public Color baseColor;
        [Range(0f,1f)] public float blendAmount;
        public Drop[] drops;

        [System.Serializable]
        public struct Drop {
            public Color color;
            public Vector2 center;
            [Range(0f, 1f)] public float throttle;
        }
    }
}
