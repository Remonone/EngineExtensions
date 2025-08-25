namespace EngineExtensions.Core.Jobs {
    public sealed class TokenBucket {
        private double _tokens, _cap, _refillPerSec, _last;
        public TokenBucket(double capacity, double refillPerSec) {
            _cap = capacity; _refillPerSec = refillPerSec; _tokens = capacity;
            _last = UnityEngine.Time.unscaledTimeAsDouble;
        }
        void Refill() {
            double now = UnityEngine.Time.unscaledTimeAsDouble;
            _tokens = System.Math.Min(_cap, _tokens + (now - _last) * _refillPerSec);
            _last = now;
        }
        
        public bool TryConsume(double cost = 1.0) {
            Refill();
            if (_tokens >= cost) { _tokens -= cost; return true; }
            return false;
        }
    }
}