using System.Collections.Generic;

namespace EngineExtensions.Abstractions.Snapshots {
    public interface ICommandSource { IEnumerable<(int tick, object command)> CommandsSince(int tickExclusive); }
    public interface ICommandReplayer { void Apply(object command); }
}