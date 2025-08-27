using System;

namespace EngineExtensions.Abstractions.Assets {
    public interface IAssetHandle<out T> : IDisposable where T : class {
        AssetId Id { get; }
        T Asset { get; }
        bool IsValid { get; }
        /// Increase ref-count (optional for pooling scenarios)
        void Retain();
        /// Decrease ref-count and possibly release underlying resources
        void Release();
    }
}