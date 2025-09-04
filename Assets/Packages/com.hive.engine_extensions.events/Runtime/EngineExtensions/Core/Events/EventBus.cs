using System.Collections.Generic;
using System.Diagnostics;
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
        
        private static readonly SortedList<IEventListener<T>> _listeners = new(new PriorityComparator());

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
            foreach (var listener in _listeners) {
                var result = executionPolicy.ExecuteHandle(() => {
                    listener.OnEvent(@event);
                    listener.OnEventNoArgs();
                });
                if (!result.Handled && !_context.IgnoreErrorLog) {
                    DefaultLogger.Instance.Write(LogLevel.ERROR, $"Error while handling event {typeof(T).Name} ", result.Exception);
                }
                if (_context.DoNotLog) return;
                DefaultLogger.Instance.Write(LogLevel.INFO, $"Event {typeof(T).Name} handled. Params: {@event}");
            }
        }
        
        
        public static void Register(IEventListener<T> listener) {
            _listeners.Add(listener);
        }
        
        public static void Unregister(IEventListener<T> listener) {
            _listeners.Remove(listener);
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
        
        private class PriorityComparator : IComparer<IEventListener<T>> {
            public int Compare(IEventListener<T> x, IEventListener<T> y) {
                if (x == null) return -1;
                if (y == null) return 1;
                Priority xEventPriority = x.GetType().GetCustomAttribute<ListenerPriorityAttribute>()?.Priority ?? Priority.LOW;
                Priority yEventPriority = y.GetType().GetCustomAttribute<ListenerPriorityAttribute>()?.Priority ?? Priority.LOW;
                return xEventPriority - yEventPriority;
            }
        }
        
        public static void Clear() {
            _listeners.Clear();
        }
    }
}