using System;
using EngineExtensions.Abstractions.Assets;
using EngineExtensions.Core.Assets.Types;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EngineExtensions.Core.Assets {
    internal sealed class AssetHandle<T> : IAssetHandle<T> where T : class {
        public AssetId Id { get; }
        public T Asset { get; private set; }
        public bool IsValid => _handle.IsValid();


        private AsyncOperationHandle<T> _handle;
        private int _refCount = 1;


        public AssetHandle(AssetId id, AsyncOperationHandle<T> handle){ Id=id; _handle=handle; Asset=handle.Result; }


        public void Retain(){ _refCount = Math.Max(1, _refCount+1); }
        public void Release(){ if (--_refCount <= 0) Dispose(); }


        public void Dispose(){ if (_handle.IsValid()) Addressables.Release(_handle); _refCount=0; Asset=null; }
    }


    internal sealed class InstanceHandle : IInstanceHandle {
        public AssetId SourceId { get; }
        public GameObject GameObject { get; private set; }
        public bool IsValid => GameObject != null;


        public InstanceHandle(AssetId id, GameObject go){ SourceId=id; GameObject=go; }


        public void ReleaseInstance(){ if (GameObject != null) { Addressables.ReleaseInstance(GameObject); GameObject=null; } }
        public void Dispose() => ReleaseInstance();
    }
}