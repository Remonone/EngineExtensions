using System;
using EngineExtensions.Abstractions;
using EngineExtensions.Abstractions.Net;

namespace EngineExtensions.Net.Photon {
    public sealed class PhotonTransportPun : ITransport
#if PHOTON_PUN
    , Photon.Pun.IOnEventCallback
#endif
    {
        public event Action<byte,int,ReadOnlyMemory<byte>> OnMessage;
        public void Dispose() {
#if PHOTON_PUN
      Photon.Pun.PhotonNetwork.RemoveCallbackTarget(this);
#endif
        }
        public PhotonTransportPun() {
#if PHOTON_PUN
      Photon.Pun.PhotonNetwork.AddCallbackTarget(this);
#endif
        }
        public void Send(byte code, ReadOnlySpan<byte> payload, Reliability reliability, byte channel = 0) {
#if PHOTON_PUN
      var opts = new ExitGames.Client.Photon.SendOptions { Reliability = reliability == Reliability.Reliable, Channel = channel };
      var reOpts = new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.Others }; // broadcast to others
      var arr = payload.ToArray();
      Photon.Pun.PhotonNetwork.RaiseEvent(code, arr, reOpts, opts);
#else
            throw new NotSupportedException("Photon PUN not available. Define PHOTON_PUN and add PUN package.");
#endif
        }
#if PHOTON_PUN
    public void OnEvent(ExitGames.Client.Photon.EventData eventData) {
      if (eventData.CustomData is byte[] bytes) {
        OnMessage?.Invoke(eventData.Code, eventData.Sender, bytes);
      }
    }
#endif
    }
}
