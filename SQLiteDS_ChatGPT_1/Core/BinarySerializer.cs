using System.IO;

namespace SQLiteDS_ChatGPT_1.Core
{
    public static class BinarySerializer
    {
        public static byte[] SerializeFuture(string code, long time, decimal price)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(code);
            bw.Write(time);
            bw.Write((double)price);
            return ms.ToArray();
        }
    }
}
