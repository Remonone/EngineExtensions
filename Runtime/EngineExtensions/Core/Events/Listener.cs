using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Events;

namespace EngineExtensions.Core.Events {
    public class Listener<T> : IEventListener<T> where T : IEvent {
        public Action<T> OnEvent { get; set; } = _ => {};
        public Action OnEventNoArgs { get; set; } = () => {};

        public void Add(Action onEvent) => OnEventNoArgs += onEvent;
        public void Remove(Action onEvent) => OnEventNoArgs -= onEvent;
        
        public void Add(Action<T> onEvent) => OnEvent += onEvent;
        public void Remove(Action<T> onEvent) => OnEvent -= onEvent;
    }
}