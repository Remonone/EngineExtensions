using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;
using EngineExtensions.Abstractions.Telemetry;
using EngineExtensions.Net.Channels;

namespace EngineExtensions.Net.Photon {
    public sealed class PhotonNetAdapter : INetAdapter {
        public INetTick Time { get; }
        public ICommandChannel Commands { get; }
        public ISnapshotChannel Snapshots { get; }
        public IControlChannel Control { get; }
        public NetStats Stats => _stats;

        public event Action<TelemetryMarker> OnMarker;

        private readonly ITransport _transport;
        private readonly NetConfig _config;
        private NetStats _stats;

        public PhotonNetAdapter(NetConfig config) {
            _config = config;
            _transport = new PhotonTransportPun();
            Time = new PhotonTickSource(new TickRate(config.TickRate));
            Func<double> now = () => Time.TimeNowSeconds;
            Action<TelemetryMarker> emit = m => OnMarker?.Invoke(m);

            double sendInterval = Math.Max(1.0 / Math.Max(1, config.SendRate), 0.001);
            Commands = new CommandChannel(_transport, config.CommandCode, config.CommandsChannel, Reliability.UNRELIABLE, sendInterval, now, emit);
            Snapshots = new SnapshotChannel(_transport, config.SnapshotCode, config.SnapshotsChannel, Reliability.UNRELIABLE, sendInterval, now, emit);
            Control = new ControlChannel(_transport, config, now, emit);
        }

        /// Call periodically (e.g., in Presentation) to flush aggregates.
        public void Pump() {
            (Commands as CommandChannel)?.Pump();
            (Snapshots as SnapshotChannel)?.Pump();
            (Control as ControlChannel)?.Pump();
        }

        public void Dispose() {
            (Commands as IDisposable)?.Dispose();
            (Snapshots as IDisposable)?.Dispose();
            (Control as IDisposable)?.Dispose();
            _transport.Dispose();
        }
    }
    
    internal sealed class PhotonTickSource : INetTick {
        public TickRate Rate { get; }
        public PhotonTickSource(TickRate rate) { Rate = rate; }
        public int TickNow {
            get {
#if PHOTON_PUN
        var ms = (double)Photon.Pun.PhotonNetwork.ServerTimestamp;
#else
                var ms = (double)(UnityEngine.Time.realtimeSinceStartupAsDouble * 1000.0);
#endif
                return (int)System.Math.Floor((ms / 1000.0) * Rate.Hz);
            }
        }
        public double TimeNowSeconds {
#if PHOTON_PUN
      get { return Photon.Pun.PhotonNetwork.ServerTimestamp / 1000.0; }
#else
            get { return UnityEngine.Time.realtimeSinceStartupAsDouble; }
#endif
        }
        public double TickToTime(int tick) => tick / (double)Rate.Hz;
        public int TimeToTick(double seconds) => (int)System.Math.Floor(seconds * Rate.Hz);
    }
}