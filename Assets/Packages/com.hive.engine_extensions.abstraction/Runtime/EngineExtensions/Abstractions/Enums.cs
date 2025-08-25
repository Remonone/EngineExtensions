using System;

namespace EngineExtensions.Abstractions {
    public enum UpdatePhase { EARLY, INPUT, NET_FIXED, LATE, PRESENTATION }
	public enum LogLevel { TRACE, DEBUG, INFO, WARNING, ERROR, FATAL }
	public enum TelemetryKind { SPAN_START, SPAN_END, COUNTER, GAUGE, EVENT }
	public enum JobPriority { LOW = 2, MEDIUM = 1, HIGH = 0 }
	public enum MsgType : byte { NONE = 0, COMMAND = 1, SNAPSHOT = 2, CONTROL = 3 }
	public enum Reliability { UNRELIABLE = 0, RELIABLE = 1 }

	[Flags] public enum TransformF : byte { X=1<<0, Y=1<<1, Z=1<<2, Yaw=1<<3 }
	[Flags] public enum VelocityF : byte { VX=1<<0, VY=1<<1, VZ=1<<2 }
	[Flags] public enum StatF : byte { HP=1<<0, MP=1<<1 }
}