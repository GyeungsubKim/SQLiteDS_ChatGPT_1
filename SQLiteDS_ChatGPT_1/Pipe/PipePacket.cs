using SQLiteDS_ChatGPT_1.Core;

namespace SQLiteDS_ChatGPT_1.Pipe
{
    public class PipePacket
    {
        public WorkType Type { get; set; }
        public long Timestamp { get; set; }
        public byte[] Payload { get; set; } = Array.Empty<byte>();
    }
}
