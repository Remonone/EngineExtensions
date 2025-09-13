using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Events;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Events.Policies.Dispatcher;
using EngineExtensions.Core.Events.Policies.Errors;
using EngineExtensions.Core.Events.Utils;
using EngineExtensions.Logger;

namespace EngineExtensions.Core.Events {
    public static class EventBus<T> where T : IEvent {
        
        private static readonly SortedList<ListenerCell> _listeners = new(new PriorityComparator());

        private static readonly EventBusContext<T> _context = new();
        
        public static void Raise(T @event) {
            if (_context.DispatchPolicy.Delayed) {
                EventRouter.Enqueue(new Envelope(_context.DispatchPolicy.Phase, @event));
                return;
            }
            Process(@event);
        }

        private static void Process(T @event) {
            var executionPolicy = ErrorPolicyTable.GetHandler(_context.ErrorPolicy);
            foreach (var cell in _listeners) {
                var result = executionPolicy.ExecuteHandle(() => {
                    cell.Listener.OnEvent(@event);
                    cell.Listener.OnEventNoArgs();
                });
                if (!result.Handled && !_context.IgnoreErrorLog) {
                    DefaultLogger.Instance.Write(LogLevel.ERROR, $"Error while handling event {typeof(T).Name} ", result.Exception);
                }
                if (_context.DoNotLog) return;
                DefaultLogger.Instance.Write(LogLevel.INFO, $"Event {typeof(T).Name} handled. Params: {@event}");
            }
        }
        
        
        public static void Register(IEventListener<T> listener, object owner) {
            ListenerCell cell = new ListenerCell(listener, owner);
            _listeners.Add(cell);
        }
        
        public static void Unregister(IEventListener<T> listener) {
            ListenerCell cell = _listeners.FirstOrDefault(l => ReferenceEquals(l.Listener, listener));
            _listeners.Remove(cell);
        }

        private readonly struct Envelope : IDefferedEnvelope {
            private readonly T _event;
            public UpdatePhase Phase { get; }
            public void DispatchNow() => Process(_event);
            
            public Envelope(UpdatePhase phase, T @event) {
                Phase = phase;
                _event = @event;
            }
        }

        private class ListenerCell {
            public readonly IEventListener<T> Listener;
            public readonly object Owner;

            public ListenerCell(IEventListener<T> listener, object owner) {
                Listener = listener;
                Owner = owner;
            }
        }
        
        private class PriorityComparator : IComparer<ListenerCell> {
            public int Compare(ListenerCell x, ListenerCell y) {
                if (x == null) return -1;
                if (y == null) return 1;
                Priority xEventPriority = ResolvePriorityFromOwnerField(x.Owner, x.Listener) ?? Priority.LOW;
                Priority yEventPriority = ResolvePriorityFromOwnerField(y.Owner, y.Listener) ?? Priority.LOW;
                return xEventPriority - yEventPriority;
            }
            
            private static Priority? ResolvePriorityFromOwnerField(object owner, IEventListener<T> instance) {
                    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    foreach (var f in owner.GetType().GetFields(flags)) {
                        if (!typeof(IEventListener<T>).IsAssignableFrom(f.FieldType)) continue;
                        var value = f.GetValue(owner);
                        if (!ReferenceEquals(value, instance)) continue;
            
                        var attr = f.GetCustomAttribute<ListenerPriorityAttribute>(inherit: true);
                        return attr?.Priority;
                    }
                    return null;
                }
        }
        
        public static void Clear() {
            _listeners.Clear();
        }
    }
}