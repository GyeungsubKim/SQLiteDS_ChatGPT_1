using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Models;
using System.Threading.Channels;

namespace SQLiteDS_ChatGPT_1.Database
{
    internal sealed class DbWorker 
    {
        private readonly Dictionary<WorkType, SqliteCommand> _cmdCache = new(); 
        private readonly Channel<(object, WorkType)> _ch;
        private readonly Database _db;
        private bool _running;

        public DbWorker(Database db)
        {
            _db = db;
            _ch = Channel.CreateBounded<(object, WorkType)>(
                new BoundedChannelOptions(1_000_000)
                {
                    SingleReader = true,
                    SingleWriter = false,
                });
        }
        public void Enqueue(object data, WorkType type)
            => _ch.Writer.TryWrite((data, type));
        public void Start()
        {
            _running = true;
            Task.Run(Run);
        }
        public async Task Run()
        {
            using var con = _db.Open();
            PrepareCommands(con);

            var batch = new List<(object, WorkType)>(1000);

            while (_running)
            {
                var item = await _ch.Reader.ReadAsync();
                batch.Add(item);

                while(_ch.Reader.TryRead(out var more))
                {
                    batch.Add(more);
                    if (batch.Count >= 500) break;
                }

                using var tran = con.BeginTransaction();

                foreach (var (data, type) in batch)
                {
                    using var cmd = _cmdCache[type];
                    cmd.Transaction = tran;
                    Bind(cmd, data);
                }
                tran.Commit();
                batch.Clear();
            }
        }
        private static void Bind(SqliteCommand cmd, object data)
        {
            var props = data.GetType().GetProperties();

            try
            {
                int i = 0;
                foreach (var p in props)
                {
                    if (p.Name == "Idx") continue;
                    cmd.Parameters[i++].Value = p.GetValue(data) ?? DBNull.Value;
                }
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ViewModel.AddLog($"Bind Data Error : {ex.Message}");
            }
        }
        private void PrepareCommands(SqliteConnection con)
        {
            foreach (WorkType type in Enum.GetValues<WorkType>())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText = type.GetInsertSql();

                foreach (var c in type.GetColums())
                    cmd.Parameters.Add(new SqliteParameter($"@{c}", null));

                _cmdCache[type] = cmd;
            }
        }
    }
}
