using System;

namespace EngineExtensions.Abstractions.Assets {
    public interface IAssetsTelemetrySink {
        void OnLoadStart(AssetId id, Type type);
        void OnLoadComplete(AssetId id, Type type, double ms, bool fromCache, bool ok, string error = null);
        void OnDownloadStart(AssetLabel label);
        void OnDownloadComplete(AssetLabel label, long bytes, double ms, bool ok, string error = null);
    }
}