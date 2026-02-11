using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Database;
using SQLiteDS_ChatGPT_1.Pipe;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Engine
{
    public sealed class FeedBus
    {
        private readonly SQLiteWriter _db;
        private readonly BinaryPipeServer _pipe;

        public Action<(WorkType, string)>? OnCounter;

        public FeedBus(SQLiteWriter db, BinaryPipeServer pipe)
        {
            _db = db;
            _pipe = pipe;

        }
        public void OnReceive(object data, WorkType type)
        {
            _db.Enqueue(data, type);
            _pipe.Publish(data, type);

            string index = "0";
            if (type == WorkType.Ksp)
            {
                var ksp = (KSP)data;
                index = ksp.Code!;
            }
            OnCounter?.Invoke((type, index));
        }
    }
}
