using System;

namespace EngineExtensions.Abstractions {
    public enum UpdatePhase { EARLY, INPUT, NET_FIXED, LATE, PRESENTATION }
	public enum LogLevel { TRACE, DEBUG, INFO, WARNING, ERROR, FATAL }
	public enum TelemetryKind { SPAN_START, SPAN_END, COUNTER, GAUGE, EVENT }
	public enum JobPriority { LOW = 2, MEDIUM = 1, HIGH = 0 }
	public enum MsgType : byte { NONE = 0, COMMAND = 1, SNAPSHOT = 2, CONTROL = 3 }
	public enum Reliability { UNRELIABLE = 0, RELIABLE = 1 }
	
	public enum ReplayBlockType : byte { BOOT=1, CMDS=2, SNAP=3, INDEX=0x7F }

}