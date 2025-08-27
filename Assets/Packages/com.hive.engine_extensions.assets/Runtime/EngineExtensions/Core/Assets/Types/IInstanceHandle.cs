using System;
using EngineExtensions.Abstractions.Assets;
using UnityEngine;

namespace EngineExtensions.Core.Assets.Types {
    public interface IInstanceHandle : IDisposable {
        AssetId SourceId { get; }
        GameObject GameObject { get; }
        bool IsValid { get; }
        void ReleaseInstance();
    }
}