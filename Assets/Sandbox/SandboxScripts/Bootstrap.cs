using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Core.Scheduler;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    
    private Scheduler _scheduler;
    private void Awake() {
        _scheduler = new Scheduler();
    }

    private void Update() {
        _scheduler.RunPhase(UpdatePhase.INPUT, Time.deltaTime);
        _scheduler.RunPhase(UpdatePhase.EARLY, Time.deltaTime);
        _scheduler.RunPhase(UpdatePhase.NET_FIXED, Time.deltaTime);
        _scheduler.RunPhase(UpdatePhase.LATE, Time.deltaTime);
        _scheduler.RunPhase(UpdatePhase.PRESENTATION, Time.deltaTime);
    }
}
