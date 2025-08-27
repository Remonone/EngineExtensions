using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EngineExtensions.Abstractions.Events;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Events.Utils;

namespace EngineExtensions.Core.Events.Wiring {
    public sealed class TlvWireEventCodec<TEvent> : IWireEventCodecBase where TEvent : class, new() {
    public uint TypeId { get; }
    public Type EventType => typeof(TEvent);

    private readonly string _eventName; // semantic name, may be null
    private readonly MemberMeta[] _members; // ordered by tag
    private readonly Dictionary<uint, MemberMeta> _byTag;

    public TlvWireEventCodec(uint typeId, string eventName){
      TypeId = typeId; _eventName = eventName ?? typeof(TEvent).Name;
      _members = BuildMembers(); _byTag = _members.ToDictionary(m=>m.Tag);
    }

    public byte[] EncodeObject(object e){ var w = new ByteWriter(64); Write(ref w, (TEvent)e); return w.ToArray(); }
    public object DecodeObject(ReadOnlySpan<byte> payload){ var r = new TlvReader(payload); var inst = new TEvent(); ReadInto(ref r, inst); return inst; }

    private void Write(ref ByteWriter w, TEvent e){
      foreach (var m in _members){
        var val = m.Getter(e);
        if (!m.Required && IsDefault(val, m.MemberType, m.DefaultValue)) continue;
        m.Write(ref w, val);
      }
    }

    private void ReadInto(ref TlvReader tlv, TEvent inst){
      while (tlv.TryRead(out var tag, out var val)){
        if (_byTag.TryGetValue(tag, out var m)) m.ReadInto(inst, val);
        // unknown tag → skip
      }
    }

    // -------------------- member meta --------------------
    private sealed class MemberMeta {
      public delegate void WriteFunc(ref ByteWriter w, object v);
      public delegate void ReadIntoFunc(TEvent e, ReadOnlySpan<byte> v);
      public uint Tag; public bool Required; public object DefaultValue; public Type MemberType;
      public Func<TEvent, object> Getter; public Action<TEvent, object> Setter;
      public WriteFunc Write;
      public ReadIntoFunc ReadInto;
    }

    private MemberMeta[] BuildMembers(){
      var t = typeof(TEvent);
      var list = new List<MemberMeta>();

      // helper: create meta for a member
      MemberMeta Build(Type memberType, uint tag, bool required, object def, Func<TEvent,object> getter, Action<TEvent,object> setter){
        var m = new MemberMeta{ Tag=tag, Required=required, DefaultValue=def, MemberType=memberType, Getter=getter, Setter=setter };
        // writers
        if (memberType == typeof(uint))       m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteU32(ref w, tag, (uint)v);
        else if (memberType == typeof(ulong)) m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteU64(ref w, tag, (ulong)v);
        else if (memberType == typeof(int))   m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteI32(ref w, tag, (int)v);
        else if (memberType == typeof(long))  m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteI64(ref w, tag, (long)v);
        else if (memberType == typeof(bool))  m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteBool(ref w, tag, (bool)v);
        else if (memberType == typeof(byte[]))m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteBytes(ref w, tag, (byte[])v ?? Array.Empty<byte>());
        else if (memberType == typeof(string))m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteString(ref w, tag, (string)v ?? string.Empty);
        else if (memberType == typeof(ushort))m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteU32(ref w, tag, (ushort)v);
        else if (memberType == typeof(short)) m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteI32(ref w, tag, (short)v);
        else if (memberType == typeof(byte))  m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteU32(ref w, tag, (byte)v);
        else if (memberType == typeof(sbyte)) m.Write = (ref ByteWriter w, object v)=> TlvWriter.WriteI32(ref w, tag, (sbyte)v);
        else throw new NotSupportedException($"Unsupported TLV member type {memberType.Name}");

        // readers
        if (memberType == typeof(uint))        m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadU32(v));
        else if (memberType == typeof(ulong))  m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadU64(v));
        else if (memberType == typeof(int))    m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadI32(v));
        else if (memberType == typeof(long))   m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadI64(v));
        else if (memberType == typeof(bool))   m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadBool(v));
        else if (memberType == typeof(byte[])) m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, v.ToArray());
        else if (memberType == typeof(string)) m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, TlvReader.ReadString(v));
        else if (memberType == typeof(ushort)) m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, (ushort)TlvReader.ReadU32(v));
        else if (memberType == typeof(short))  m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, (short)TlvReader.ReadI32(v));
        else if (memberType == typeof(byte))   m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, (byte)TlvReader.ReadU32(v));
        else if (memberType == typeof(sbyte))  m.ReadInto = (TEvent e, ReadOnlySpan<byte> v)=> m.Setter(e, (sbyte)TlvReader.ReadI32(v));
        return m;
      }

      // determine event semantic name
      var eventName = _eventName ?? typeof(TEvent).Name;

      // fields
      foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)){
        var wf  = f.GetCustomAttribute<WireFieldAttribute>();
        if (wf==null) continue;
        if (f.IsInitOnly) throw new InvalidOperationException($"Field {f.Name} must be settable");
        uint tag = wf.Tag ?? HashUtil.Fnv1a32Lower(eventName + "." + (wf.Key ?? f.Name));
        list.Add(Build(f.FieldType, tag, wf.Required, wf?.DefaultValue, e=>f.GetValue(e), (e,v)=>f.SetValue(e,v)));
      }
      // properties
      foreach (var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)){
        var wf  = p.GetCustomAttribute<WireFieldAttribute>();
        if (wf==null) continue;
        if (!p.CanRead || !p.CanWrite) throw new InvalidOperationException($"Property {p.Name} must be readable & writeable");
        uint tag = wf.Tag ?? HashUtil.Fnv1a32Lower(eventName + "." + (wf.Key ?? p.Name));
        list.Add(Build(p.PropertyType, tag, wf.Required, wf.DefaultValue, e=>p.GetValue(e), (e,v)=>p.SetValue(e,v)));
      }

      if (list.Count==0) throw new InvalidOperationException($"Type {t.Name} has no [Tlv]/[WireField] members");
      // ensure unique tags within event
      var dup = list.GroupBy(m=>m.Tag).FirstOrDefault(g=>g.Count()>1);
      if (dup!=null) throw new InvalidOperationException($"Duplicate TLV tag {dup.Key} in {t.Name}. Use WireField.Tag to disambiguate.");
      list.Sort((x,y)=> x.Tag.CompareTo(y.Tag));
      return list.ToArray();
    }

    private static bool IsDefault(object val, Type t, object customDefault){
      if (customDefault != null) return Equals(val, customDefault);
      if (t.IsValueType) return Equals(val, Activator.CreateInstance(t));
      return val == null;
    }
  }
}