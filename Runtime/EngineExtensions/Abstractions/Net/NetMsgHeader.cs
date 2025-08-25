using EngineExtensions.Abstractions.Utils;

namespace EngineExtensions.Abstractions.Net {
    public readonly struct NetMsgHeader {
        public const byte SchemaVersion = 1;
        public readonly byte Schema;       // for forward-compat
        public readonly MsgType Type;      // command / snapshot / control
        public readonly int Tick;          // simulation tick
        public readonly uint Seq;          // optional sequence id (0 if unused)
        public readonly uint AckMask;      // optional ack bitmask (0 if unused)

        public NetMsgHeader(MsgType type, int tick, uint seq = 0, uint ackMask = 0) {
            Schema = SchemaVersion; Type = type; Tick = tick; Seq = seq; AckMask = ackMask;
        }

        public static void Write(ref ByteWriter w, in NetMsgHeader h) {
            w.WriteByte(h.Schema);
            w.WriteByte((byte)h.Type);
            w.WriteVarUInt((uint)h.Tick);
            w.WriteVarUInt(h.Seq);
            w.WriteVarUInt(h.AckMask);
        }

        public static NetMsgHeader Read(ref ByteReader r) {
            byte schema = r.ReadByte();
            var type = (MsgType)r.ReadByte();
            uint tick = r.ReadVarUInt();
            uint seq = r.ReadVarUInt();
            uint ack = r.ReadVarUInt();
            return new NetMsgHeader(type, (int)tick, seq, ack) { };
        }
    }
}