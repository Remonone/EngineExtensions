using System;
using System.Collections.Generic;
using System.Reflection;
using EngineExtensions.Abstractions.Events;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Events.Utils;
using EngineExtensions.Core.Events.Wiring;

namespace EngineExtensions.Core.Events.Registry {
    public sealed class WireEventRegistry {
    private readonly Dictionary<uint, IWireEventCodecBase> _byId = new();
    private readonly Dictionary<Type, IWireEventCodecBase> _byType = new();

    public void Register<T>(uint typeId, string eventName=null) where T : class, new(){
        var c = new TlvWireEventCodec<T>(typeId, eventName);
        _byId[typeId] = c; _byType[typeof(T)] = c;
    }
    

    public void AutoRegisterFrom(params Assembly[] asms){
        foreach (var a in asms){
            foreach (var t in a.GetTypes()) {
                if (!typeof(IEvent).IsAssignableFrom(t)) continue;
                var attr = t.GetCustomAttribute<WireEventAttribute>(); 
                if (attr==null) continue;
                if (!t.IsClass || t.IsAbstract) continue;
                if (t.GetConstructor(Type.EmptyTypes)==null) throw new InvalidOperationException($"{t.Name} must have parameterless ctor for TLV");
                uint typeId = attr.TypeId ?? HashUtil.Fnv1a32Lower(attr.Name ?? t.FullName);
                var codecType = typeof(TlvWireEventCodec<>).MakeGenericType(t);
                var codec = (IWireEventCodecBase)Activator.CreateInstance(codecType, typeId, attr.Name ?? t.Name);
                if (_byId.ContainsKey(typeId)) throw new InvalidOperationException($"WireEvent type id collision for {attr.Name ?? t.Name} (0x{typeId:X8}). Provide explicit TypeId in [WireEvent].");
                _byId[typeId] = codec; _byType[t] = codec;
            }
        }
    }

    public bool TryGetByType(Type t, out IWireEventCodecBase c) => _byType.TryGetValue(t, out c);
    public bool TryGetById(uint id, out IWireEventCodecBase c) => _byId.TryGetValue(id, out c);

    public byte[] EncodeObject(object evt){
        if (!_byType.TryGetValue(evt.GetType(), out var c))
            throw new InvalidOperationException($"No codec registered for {evt.GetType().Name}");
        return c.EncodeObject(evt);
    }
    public object DecodeObject(uint typeId, ReadOnlySpan<byte> payload){
        if (!_byId.TryGetValue(typeId, out var c)) 
            throw new InvalidOperationException($"No codec for event type {typeId}");
        return c.DecodeObject(payload);
    }
  }
}