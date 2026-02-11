using Microsoft.Data.Sqlite;
using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Models;

namespace SQLiteDS_ChatGPT_1.Database
{
    public sealed class Database(string path) : IDisposable
    {
        private readonly string _conn = $"Data Source={path}";
        private readonly Dictionary<WorkType, SqliteCommand> _cmdCache = new();

        public void Init()
        {
            using var con = new SqliteConnection(_conn);
            con.Open();

            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA temp_store=MEMORY;
                PRAGMA cache_size=-200000;
                PRAGMA mmap_size=30000000000;
                PRAGMA page_size=32768;
                PRAGMA locking_mode=EXCLUSIVE;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlCreateGenerator.GenerateAllTablesSql();
            cmd.ExecuteNonQuery();

            //PrepareCommands(con);
        }
        public SqliteConnection Open()
        {
            var con = new SqliteConnection(_conn);
            con.Open();
            return con;
        }
        public void WriteBatch(List<(object Data, WorkType Type)> batch)
        {
            using var con = new SqliteConnection(_conn);
            con.Open();

            using var tran = con.BeginTransaction();

            foreach (var (data, type) in batch)
            {
                var cmd = _cmdCache[type];
                cmd.Transaction = tran;
                Bind(cmd, data);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ViewModel.AddLog($"Insert Data Error : {ex.Message}");
                }
            }
            tran.Commit();
        }
        private void Bind(SqliteCommand cmd, object data)
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
        public void Dispose() { }
    }
}
