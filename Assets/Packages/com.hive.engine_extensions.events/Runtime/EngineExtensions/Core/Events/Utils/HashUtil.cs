using System.Text;

namespace EngineExtensions.Core.Events.Utils {
    public class HashUtil {

        public static uint Fnv1a32Lower(string s) {
            unchecked {
                uint h = 2166136261;
                var bytes = Encoding.UTF8.GetBytes(s.ToLowerInvariant());
                for (int i = 0; i < bytes.Length; i++) {
                    h ^= bytes[i];
                    h *= 16777619;
                }

                return h;
            }
        }
        
    }
}