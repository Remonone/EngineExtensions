using System;

namespace EngineExtensions.Abstractions.Assets {
    [Serializable]
    public struct AssetId : IEquatable<AssetId> {
        public string Value;
        public AssetId(string v){ Value = v; }
        public static AssetId From(string v) => new AssetId(v);
        public override string ToString() => Value ?? string.Empty;
        public bool Equals(AssetId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is AssetId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public static implicit operator AssetId(string v) => new AssetId(v);
    }


    /// Label for bulk operations (preload, download, release) without revealing provider-specific API
    [Serializable]
    public struct AssetLabel : IEquatable<AssetLabel> {
        public string Value;
        public AssetLabel(string v){ Value = v; }
        public static AssetLabel From(string v) => new AssetLabel(v);
        public override string ToString() => Value ?? string.Empty;
        public bool Equals(AssetLabel other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is AssetLabel other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public static implicit operator AssetLabel(string v) => new AssetLabel(v);
    }
}