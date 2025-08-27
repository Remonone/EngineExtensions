using UnityEngine;

namespace EngineExtensions.Extras.Extensions {
    public static class LayerMaskExtensions {
        public static bool Contains(this LayerMask mask, int layerNumber) {
            return mask == (mask | (1 << layerNumber));
        }
    }
}