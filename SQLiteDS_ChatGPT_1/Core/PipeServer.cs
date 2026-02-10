using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Core
{
    public class PipeServer(string name)
    {
        private readonly string _name = name;
        private readonly List<Client> _clients = [];

        class Client
        {
            public NamedPipeServerStream Stream = null!;
            public Channel<byte[]> Queue = Channel.CreateBounded<byte[]>(
                new BoundedChannelOptions(50_000)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                });
        }
        public void Start()
            => Task.Run(Listen);
        private async Task Listen()
        {
            while (true)
            {
                var server = new NamedPipeServerStream(
                    _name,
                    PipeDirection.Out,
                    20,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync();
                var client = new Client { Stream = server };
                lock(_clients)
                    _clients.Add(client);
                _ = Task.Run(() => Sender(client));
            }
        }
        private async Task Sender(Client c)
        {
            await foreach (var msg in c.Queue.Reader.ReadAllAsync())
            {
                if (!c.Stream.IsConnected) break;
                await c.Stream.WriteAsync(msg);
            }
        }
        public void Broadcast(object data, WorkType type)
        {
            var json = JsonSerializer.Serialize(new { Type = type, Data = data });
            var bytes = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(bytes.Length);
            var packet = len.Concat(bytes).ToArray();

            lock (_clients)
                foreach (var c in _clients)
                    c.Queue.Writer.TryWrite(packet);
        }
    }
}
