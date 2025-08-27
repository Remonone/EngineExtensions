using System;
using EngineExtensions.Abstractions.Utils;
using EngineExtensions.Input.Commands;
using NUnit.Framework;
using UnityEngine;

namespace EngineExtensions.Tests {

    sealed class JumpCodec : ICommandCodec {
        public const ushort Id=1; 
        public ushort TypeId=>Id; 
        public bool IsEdgeTriggered=>true;

        public void Write(ref ByteWriter w, ReadOnlySpan<byte> _) {
            w.WriteByte(0);
        }

        public byte[] Read(ref ByteReader r) {
            r.ReadByte(); 
            return Array.Empty<byte>();
        }
    }

    sealed class FireCodec : ICommandCodec {
        public const ushort Id=2; 
        public ushort TypeId=>Id; 
        public bool IsEdgeTriggered=>true; 
        public static byte[] Canon(byte power)=> new[]{ power };

        public void Write(ref ByteWriter w, ReadOnlySpan<byte> c) {
            w.WriteByte(c.Length>0?c[0]:(byte)0);
        }

        public byte[] Read(ref ByteReader r) {
            return new[]{ r.ReadByte() };
        }
    }
    
    sealed class MoveCodec : ICommandCodec {
        public const ushort Id=3; 
        public ushort TypeId=>Id; 
        public bool IsEdgeTriggered=>true;

        public static byte[] Canon(ushort x, ushort y) {
            var w = new ByteWriter(8);
            w.WriteVarUInt(ZigZagEncoder.Encode(x)); 
            w.WriteVarUInt(ZigZagEncoder.Encode(y)); 
            return w.ToArray();
        }

        public void Write(ref ByteWriter w, ReadOnlySpan<byte> c) {
            w.WriteByte(c.Length>0?c[0]:(byte)0);
        }

        public byte[] Read(ref ByteReader r) {
            return new[]{ r.ReadByte() };
        }
    }
    
    public class CommandPacketTest {
        [Test]
        public void CheckForCommandTransfer() {
            var reg = new CommandRegistry(); reg.Register(new JumpCodec()); reg.Register(new FireCodec());
            var writer = new CommandPacketWriter(reg); var reader = new CommandPacketReader(reg);
            var cmds = new (ushort, byte[])[] { (JumpCodec.Id, Array.Empty<byte>()), (FireCodec.Id, FireCodec.Canon(7)) };
            var bytes = writer.Write(123, 42, cmds);
            var pkt = reader.Read(bytes);
            Assert.AreEqual(123, pkt.Tick); Assert.AreEqual(42, pkt.PlayerId); Assert.AreEqual(2, pkt.Commands.Length);
            Assert.AreEqual(JumpCodec.Id, pkt.Commands[0].TypeId);
            Assert.AreEqual(0, pkt.Commands[0].Canonical.Length);
            Assert.AreEqual(FireCodec.Id, pkt.Commands[1].TypeId);
            Assert.AreEqual(7, pkt.Commands[1].Canonical[0]);
        }
    }
}