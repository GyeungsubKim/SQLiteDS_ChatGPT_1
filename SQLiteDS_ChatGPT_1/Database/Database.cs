using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SQLiteDS_ChatGPT_1.Database
{
    public class Database(string path) : IDisposable
    {
        private readonly string _conn = $"Data Source={path}";
        private readonly Channel<(object, WorkType)> _ch =
            Channel.CreateBounded<(object, WorkType)>( new BoundedChannelOptions(200_000)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        private readonly CancellationTokenSource _cts = new();
        private Task[]? _workers;

        public void Init()
        {
            using var con = new SqliteConnection(_conn);
            con.Open();

            var cmd = con.CreateCommand();

            string pragma = @"PRAGMA journal_mode=WAL;";
            pragma += "PRAGMA synchronous=NORMAL;";
            pragma += "PRAGMA temp_store=MEMORY;";
            pragma += "PRAGMA mmap_size=30_000_000_000;";
            pragma += "PRAGMA cache_size=-200_000;";
            pragma += "PRAGMA page_size=32768;";
            pragma += "PRAGME locking_mode=EXCLUSIVE;";
            cmd.CommandText = pragma;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlCreateGenerator.GenerateAllTablesSql();
            cmd.ExecuteNonQuery();
        }
        public void Start(int workerCount = 4)
        {
            _workers = new Task[workerCount];
            for (int i = 0; i < workerCount; i++) 
                _workers[i] = Task.Run(Worker);
        }
        public async Task EnqueueData<T>(T data, WorkType type)
            => await _ch.Writer.WriteAsync((data!, type));
        private async Task Worker()
        {
            using var con = new SqliteConnection(_conn);
            con.Open();

            Dictionary<WorkType, SqliteCommand> localCache = [];

            PrepareCommands(con, localCache);

            var batch = new List<(object, WorkType)>(1_000);

            while (!_cts.IsCancellationRequested)
            {
                var item = await _ch.Reader.ReadAsync(_cts.Token);
                batch.Add(item);

                while (_ch.Reader.TryRead(out var more))
                {
                    batch.Add(more);
                    if (batch.Count >= 500) break;
                }

                using var trans = con.BeginTransaction();

                foreach (var (obj, type) in batch)
                {
                    var cmd = localCache[type];
                    cmd.Transaction = trans;
                    Bind(cmd, obj, type);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        ViewModel.AddLog($"ExecuteNonQuery Error : {ex.Message}");
                    }
                }

                trans.Commit();
                batch.Clear();
            }
        }
        private void Bind(SqliteCommand cmd, object obj, WorkType type)
        {
            cmd.Parameters.Clear();
            foreach (var col in type.GetColums())
            {
                var value = obj.GetType().GetProperty(col)?.GetValue(obj);
                cmd.Parameters.AddWithValue($"@{col}", value ?? DBNull.Value);
            }
        }
        private void PrepareCommands(SqliteConnection con, Dictionary<WorkType, SqliteCommand> cache)
        {
            foreach (WorkType t in Enum.GetValues(typeof(WorkType))) 
            {
                var cmd = con.CreateCommand();
                cmd.CommandText = t.GetInsertSql();
                cache[t] = cmd;
            }
        }
        public void Dispose()
        {
            _cts.Cancel();
            _workers?.ToList().ForEach(t => t.Wait());
            GC.SuppressFinalize(this);
        }
    }
}
