using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Models;
using System.ComponentModel;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Database
{
    public sealed class SQLiteWriter(Database db)
    {
        private readonly DbWorker _worker = new DbWorker(db);

        public void Start() => _worker.Start();
        public void Enqueue(object data, WorkType type)
            => _worker.Enqueue(data, type);
    }
}
