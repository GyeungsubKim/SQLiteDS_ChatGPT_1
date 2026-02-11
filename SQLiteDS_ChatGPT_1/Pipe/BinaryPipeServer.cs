using SQLiteDS_ChatGPT_1.Core;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;

namespace SQLiteDS_ChatGPT_1.Pipe
{
    public sealed class BinaryPipeServer(string name) 
    {
        private readonly List<NamedPipeServerStream> _clients = [];
        private readonly string? _pipeName = name;
        public void Start()
            => Task.Run(() => ListenLoop());
        private async Task ListenLoop()
        {
            while (true)
            {
                var server = new NamedPipeServerStream(
                    _pipeName!,
                    PipeDirection.Out,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync();
                lock (_clients) _clients.Add(server);
            }
        }
        public void Publish(object data, WorkType type)
        {
            var packet = new PipePacket
            {
                Type = type,
                Payload = JsonSerializer.SerializeToUtf8Bytes(data)
            };
            var bytes = JsonSerializer.SerializeToUtf8Bytes(packet);

            lock (_clients)
            {
                foreach (var c in _clients.ToArray())
                {
                    if (!c.IsConnected) continue;
                    try { c.Write(bytes); }
                    catch { }
                }
            }
        }
        //public void Broadcast(object data, WorkType type)
        //{
        //    byte[] payload = JsonSerializer.SerializeToUtf8Bytes(data);

        //    foreach (var c in _clients)
        //    {
        //        if (!c.IsConnected) continue;

        //        try
        //        {
        //            c.Write(payload);
        //        }
        //        catch { }
        //    }
        //}
    }
}
