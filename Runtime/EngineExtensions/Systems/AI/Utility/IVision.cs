using System.Collections.Generic;
using UnityEngine;

namespace EngineExtensions.Systems.AI.Utility {
    public interface IVision {
        List<Transform> ObjectsInVision { get; }
        List<string> ObserveTags { get; }
    }
}