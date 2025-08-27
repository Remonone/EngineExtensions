using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Assets;
using EngineExtensions.Core.Assets.Types;
using EngineExtensions.Logger;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace EngineExtensions.Core.Assets {
    public sealed class AddressableAssets : IAssets {
        private readonly Dictionary<(Type, string), object> _cache = new();
        private readonly IAssetsTelemetrySink _telemetry;

        public AddressableAssets(IAssetsTelemetrySink telemetry = null){ _telemetry = telemetry; }

        public async Task InitializeAsync(CancellationToken ct = default){
            var t0 = Time.realtimeSinceStartupAsDouble;
            var handle = Addressables.InitializeAsync();
            await handle.Task;
            _telemetry?.OnLoadComplete(AssetId.From("#init"), typeof(void), (Time.realtimeSinceStartupAsDouble - t0)*1000, true, handle.Status == AsyncOperationStatus.Succeeded, handle.OperationException?.Message);
        }

        public bool TryGetCached<T>(AssetId id, out IAssetHandle<T> handle) where T : class {
            if (_cache.TryGetValue((typeof(T), id.Value), out var obj)) {
                handle = (IAssetHandle<T>)obj; 
                handle.Retain(); 
                return true;
            }
            handle = null; 
            return false;
        }

        public async Task<IAssetHandle<T>> LoadAsync<T>(AssetId id, CancellationToken ct = default) where T : class {
            if (TryGetCached<T>(id, out var h)) return h;

            _telemetry?.OnLoadStart(id, typeof(T));
            var t0 = Time.realtimeSinceStartupAsDouble;

            var op = Addressables.LoadAssetAsync<T>(id.Value);
            try {
                await AwaitWithCancellation(op, ct, releaseResult: _ => Addressables.Release(op), releaseHandle: Addressables.Release);
            } catch (OperationCanceledException) {
                _telemetry?.OnLoadComplete(id, typeof(T), (Time.realtimeSinceStartupAsDouble - t0)*1000, false, false, "canceled");
                throw;
            }

            var ms = (Time.realtimeSinceStartupAsDouble - t0)*1000;
            var handle = new AssetHandle<T>(id, op);
            _cache[(typeof(T), id.Value)] = handle;
            _telemetry?.OnLoadComplete(id, typeof(T), ms, false, true);
            return handle;
        }

        public async Task<IInstanceHandle> InstantiateAsync(AssetId prefabId, Vector3 position, Quaternion rotation, Transform parent = null, CancellationToken ct = default){
            _telemetry?.OnLoadStart(prefabId, typeof(GameObject));
            var t0 = Time.realtimeSinceStartupAsDouble;

            var op = Addressables.InstantiateAsync(prefabId.Value, position, rotation, parent);
            GameObject go;
            try {
                go = await AwaitWithCancellation(
                    op, ct,
                    releaseResult: g => { if (g != null) Addressables.ReleaseInstance(g); else Addressables.Release(op); }
                );
            } catch (OperationCanceledException) {
                _telemetry?.OnLoadComplete(prefabId, typeof(GameObject), (Time.realtimeSinceStartupAsDouble - t0)*1000, false, false, "canceled");
                throw;
            }
            _telemetry?.OnLoadComplete(prefabId, typeof(GameObject), (Time.realtimeSinceStartupAsDouble - t0)*1000, false, true);
            return new InstanceHandle(prefabId, go);
        }

        public async Task<AsyncOperation> LoadSceneAsync(AssetId sceneId, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true, CancellationToken ct = default){
            _telemetry?.OnLoadStart(sceneId, typeof(Scene));
            var t0 = Time.realtimeSinceStartupAsDouble;

            var op = Addressables.LoadSceneAsync(sceneId.Value, mode, activateOnLoad);
            SceneInstance inst;
            try {
                inst = await AwaitWithCancellation(op, ct, releaseResult: si => Addressables.UnloadSceneAsync(si));
            }
            catch (OperationCanceledException)
            {
                _telemetry?.OnLoadComplete(sceneId, typeof(Scene), (Time.realtimeSinceStartupAsDouble - t0)*1000, false, false, "canceled");
                throw;
            }

            _telemetry?.OnLoadComplete(sceneId, typeof(Scene), (Time.realtimeSinceStartupAsDouble - t0)*1000, false, true);
            return inst.ActivateAsync();
        }

        public async Task<long> GetDownloadSizeAsync(AssetLabel label, CancellationToken ct = default){
            var op = Addressables.GetDownloadSizeAsync(label.Value);
            await op.Task; 
            return op.Result;
        }

        public async Task DownloadDependenciesAsync(AssetLabel label, IProgress<float> progress = null, CancellationToken ct = default){
            _telemetry?.OnDownloadStart(label);
            var t0 = Time.realtimeSinceStartupAsDouble;

            var op = Addressables.DownloadDependenciesAsync(label.Value, true);
            if (progress != null)
                op.Completed += _ => progress.Report(1f);

            try {
                await AwaitWithCancellation(op, ct);
            } catch (OperationCanceledException) {
                var msC = (Time.realtimeSinceStartupAsDouble - t0)*1000;
                _telemetry?.OnDownloadComplete(label, 0, msC, false, "canceled");
                throw;
            }

            var ms = (Time.realtimeSinceStartupAsDouble - t0)*1000;
            long size = 0;
            try { var sizeOp = Addressables.GetDownloadSizeAsync(label.Value); 
                await sizeOp.Task; 
                size = sizeOp.Result; 
            } catch {
                // ignored
            }
            _telemetry?.OnDownloadComplete(label, size, ms, true);
        }

        public async Task PreloadByLabelAsync<T>(AssetLabel label, CancellationToken ct = default) where T : class {
            var locHandle = Addressables.LoadResourceLocationsAsync(label.Value, typeof(T));
            await locHandle.Task;
            if (locHandle.Status != AsyncOperationStatus.Succeeded)
                throw locHandle.OperationException ?? new Exception($"LoadResourceLocations failed: {label}");

            try {
                foreach (var loc in locHandle.Result) {
                    ct.ThrowIfCancellationRequested();
                    var id = new AssetId(loc.PrimaryKey);
                    if (_cache.ContainsKey((typeof(T), id.Value))) continue;

                    var load = Addressables.LoadAssetAsync<T>(loc);
                    await AwaitWithCancellation(load, ct, releaseResult: _ => Addressables.Release(load), releaseHandle: h => Addressables.Release(h));
                    if (load.Status == AsyncOperationStatus.Succeeded)
                        _cache[(typeof(T), id.Value)] = new AssetHandle<T>(id, load);
                    else
                        DefaultLogger.Instance.Write(LogLevel.INFO, $"Failed to preload asset {id} ({loc.PrimaryKey})");
                }
            }
            finally {
                Addressables.Release(locHandle);
            }
        }
        
        static async Task<T> AwaitWithCancellation<T>(AsyncOperationHandle<T> op, CancellationToken ct, Action<T> releaseResult, Action<AsyncOperationHandle<T>> releaseHandle = null) {
            if (!ct.CanBeCanceled)
                return await op.Task; // просто ждём
            if (op.IsDone)
                return op.Result;
            var cancelTcs = new TaskCompletionSource<bool>();
            using (ct.Register(() => cancelTcs.TrySetResult(true))) {
                var completed = await Task.WhenAny(op.Task, cancelTcs.Task);
                if (completed == cancelTcs.Task) {
                    try {
                        var res = await op.Task;
                        try { releaseResult?.Invoke(res); } catch {
                            // ignored
                        }
                    } catch {
                        try { releaseHandle?.Invoke(op); } catch {
                            // ignored
                        }
                    }
                    throw new OperationCanceledException(ct);
                }
            }
            return op.Result;
        }
        
        static async Task AwaitWithCancellation(AsyncOperationHandle op, CancellationToken ct, Action<AsyncOperationHandle> releaseHandle = null) {
            if (!ct.CanBeCanceled) { await op.Task; return; }
            if (op.IsDone) return;

            var tcs = new TaskCompletionSource<bool>();
            using (ct.Register(() => tcs.TrySetResult(true))) {
                var completed = await Task.WhenAny(op.Task, tcs.Task);
                if (completed == tcs.Task) {
                    try { await op.Task; } catch { /* ignore */ }
                    try { releaseHandle?.Invoke(op); } catch { /* ignore */ }
                    throw new OperationCanceledException(ct);
                }
            }
        }

        public void Release<T>(IAssetHandle<T> handle) where T : class { handle?.Release(); }
        public void ReleaseInstance(IInstanceHandle instance) { instance?.ReleaseInstance(); }

        public void ReleaseAll(){
            foreach (var kv in _cache.Values){
                switch (kv){
                    case IDisposable d: d.Dispose(); break;
                }
            }
            _cache.Clear();
        }
    }
}