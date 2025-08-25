using EngineExtensions.Abstractions;
using EngineExtensions.Core.Events;
using EngineExtensions.Core.Events.Attributes;
using EngineExtensions.Core.Scheduler;
using NUnit.Framework;

namespace EngineExtensions.Tests {
    
    [ErrorHandling(ErrorPolicy.SWALLOW)]
    [DoNotLog]
    public class ImmediateEvent : IEvent { }

    [PhaseDispatch(UpdatePhase.PRESENTATION, DispatchMode.DELAYED)]
    [DoNotLog]
    public class DelayedEvent : IEvent { }

    public class EventTests {
        [Test]
        public void ShouldDispatchEventImmediately() {
            var isCalled = false;
            
            Listener<ImmediateEvent> listener = new Listener<ImmediateEvent>();
            listener.Add(() => isCalled = true);
            
            EventBus<ImmediateEvent>.Register(listener);
            
            EventBus<ImmediateEvent>.Raise(new ImmediateEvent());
            
            Assert.IsTrue(isCalled);
        }

        [Test]
        public void ShouldNotRaiseEventImmediatelyIfDelayed() {
            var isCalled = false;
            
            Listener<DelayedEvent> listener = new Listener<DelayedEvent>();
            listener.Add(() => isCalled = true);
            EventBus<DelayedEvent>.Register(listener);
            
            EventBus<DelayedEvent>.Raise(new DelayedEvent());
            
            Assert.IsFalse(isCalled);
        }

        [Test]
        public void ShouldRaiseDelayedEventOnPhase() {
            var isCalled = false;
            
            Scheduler scheduler = new Scheduler();
            scheduler.Schedule(new EventPump(UpdatePhase.PRESENTATION, 0), UpdatePhase.PRESENTATION, int.MinValue);
            
            Listener<DelayedEvent> listener = new Listener<DelayedEvent>();
            listener.Add(() => isCalled = true);
            
            EventBus<DelayedEvent>.Register(listener);
            
            EventBus<DelayedEvent>.Raise(new DelayedEvent());
            scheduler.RunPhase(UpdatePhase.PRESENTATION, 0);
            
            Assert.IsTrue(isCalled);
        }
        
    }
}