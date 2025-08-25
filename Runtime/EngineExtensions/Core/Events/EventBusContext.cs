using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Events.Policies.Dispatcher;

namespace EngineExtensions.Core.Events {
    public class EventBusContext<T> where T : IEvent {
        public ErrorPolicy ErrorPolicy;
        public bool DoNotLog;
        public bool IgnoreErrorLog;
        public DispatchPolicy DispatchPolicy;

        public EventBusContext() {
            var policyAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(ErrorHandlingAttribute)) as ErrorHandlingAttribute;
            ErrorPolicy = policyAttribute?.Policy ?? ErrorPolicy.SWALLOW;
            DoNotLog = Attribute.IsDefined(typeof(T), typeof(DoNotLogAttribute));
            IgnoreErrorLog = Attribute.IsDefined(typeof(T), typeof(IgnoreErrorLogAttribute));
            DispatchPolicy = DispatchPolicy.From(typeof(T));
        }
    }
}