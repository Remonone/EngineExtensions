using System.Collections.Generic;

namespace EngineExtensions.Systems.AI.Utility {
    public interface IContextShare {
        Dictionary<string, object> ShareContext();
    }
}