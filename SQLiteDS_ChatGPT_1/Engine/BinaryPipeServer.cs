using SQLiteDS_ChatGPT_1.Core;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Engine
{
    public sealed class BinaryPipeServer
    {
        private readonly ConcurrentBag<NamedPipeServerStream> _clients = [];

        public void Start(string name) 
            => Task.Run(() => AcceptLoop(name));
        private async Task AcceptLoop(string name)
        {
            while (true)
            {
                var _pipe = new NamedPipeServerStream(
                    name,
                    PipeDirection.Out,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await _pipe.WaitForConnectionAsync();
                _clients.Add(_pipe);
            }
        }
        public void Broadcast(object data, WorkType type)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(data);

            foreach (var c in _clients)
            {
                if (!c.IsConnected) continue;

                try
                {
                    c.Write(payload);
                }
                catch { }
            }
        }
    }
}
