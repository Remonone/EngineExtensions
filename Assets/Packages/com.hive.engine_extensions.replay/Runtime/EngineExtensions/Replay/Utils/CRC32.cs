namespace EngineExtensions.Replay.Utils {
    internal static class CRC32 {
        static readonly uint[] table = Init();
        static uint[] Init(){
            var t = new uint[256];
            for (uint i=0;i<256;i++){
                uint c=i; for(int j=0;j<8;j++) c = ((c & 1) != 0) ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
                t[i]=c;
            }
            return t;
        }
        
        public static uint Compute(byte[] data, int offset, int count){
            uint crc = 0xFFFFFFFF;
            for (int i=0;i<count;i++) crc = table[(crc ^ data[offset+i]) & 0xFF] ^ (crc >> 8);
            return crc ^ 0xFFFFFFFF;
        }
    }
}