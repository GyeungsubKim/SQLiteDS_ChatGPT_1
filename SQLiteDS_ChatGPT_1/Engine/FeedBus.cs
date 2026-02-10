using SQLiteDS_ChatGPT_1.Core;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Engine
{
    public sealed class FeedBus : IDisposable
    {
        private readonly Channel<(object, WorkType)> _ch;

        private readonly Database.Database _db;
        private readonly BinaryPipeServer _pipe;

        private bool _running;

        public Action<(WorkType, string)>? OnCounter;

        public FeedBus(Database.Database db, BinaryPipeServer pipe)
        {
            _db = db;
            _pipe = pipe;

            _ch = Channel.CreateBounded<(object, WorkType)>(
                new BoundedChannelOptions(2_000_000)
                {
                    SingleReader = false,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                });
        }
        public void Start()
        {
            _running = true;
            for (int i = 0; i < 4; i++)
                Task.Run(Worker);
        }
        public void Publish(object data, WorkType type)
            => _ch.Writer.TryWrite((data, type));
        private async Task Worker()
        {
            var batch = new List<(object, WorkType)>(2_000);

            while (_running)
            {
                var item = await _ch.Reader.ReadAsync();
                batch.Add(item);

                while (_ch.Reader.TryRead(out var more))
                {
                    batch.Add(more);
                    if (batch.Count >= 1_000) break;
                }

                foreach (var (obj, type) in batch)
                {
                    await _db.EnqueueData(obj, type);

                    try
                    {
                        _pipe.Broadcast(obj, type);
                    }
                    catch { }

                    string index = "0";
                    if (type == WorkType.Ksp)
                    {
                        var ksp = (KSP)obj;
                        index = ksp.Code!;
                    }
                    OnCounter?.Invoke((type, index));
                }
                batch.Clear();
            }
        }
        public void Dispose()
        {
            _running = false;
        }
    }
}
