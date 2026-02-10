using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Core
{
    public sealed class EventBuffer<T>
    {
        private readonly Channel<T> _ch =
            Channel.CreateBounded<T>(100_000);
        public ValueTask Write(T item)
            => _ch.Writer.WriteAsync(item);
        public IAsyncEnumerable<T> ReadAll()
            => _ch.Reader.ReadAllAsync();
    }
}
