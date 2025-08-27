using System;
using System.Threading;
using System.Threading.Tasks;
using EngineExtensions.Abstractions.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EngineExtensions.Core.Assets.Types {
    public interface IAssets {
// --- Initialization ---
        Task InitializeAsync(CancellationToken ct = default);


// --- Single assets ---
        Task<IAssetHandle<T>> LoadAsync<T>(AssetId id, CancellationToken ct = default) where T : class;
        bool TryGetCached<T>(AssetId id, out IAssetHandle<T> handle) where T : class;


// --- Prefab instantiation ---
        Task<IInstanceHandle> InstantiateAsync(AssetId prefabId, Vector3 position, Quaternion rotation, Transform parent = null, CancellationToken ct = default);


// --- Scenes ---
        Task<AsyncOperation> LoadSceneAsync(AssetId sceneId, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true, CancellationToken ct = default);


// --- Bulk / labels ---
        Task<long> GetDownloadSizeAsync(AssetLabel label, CancellationToken ct = default);
        Task DownloadDependenciesAsync(AssetLabel label, IProgress<float> progress = null, CancellationToken ct = default);
        Task PreloadByLabelAsync<T>(AssetLabel label, CancellationToken ct = default) where T : class; // Loads to cache but doesn't instantiate


// --- Maintenance ---
        void Release<T>(IAssetHandle<T> handle) where T : class;
        void ReleaseInstance(IInstanceHandle instance);
        void ReleaseAll();
    }
}