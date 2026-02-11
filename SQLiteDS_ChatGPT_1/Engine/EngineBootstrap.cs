using Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal;
using SQLiteDS_ChatGPT_1.Daishin;
using SQLiteDS_ChatGPT_1.Database;
using SQLiteDS_ChatGPT_1.Pipe;

namespace SQLiteDS_ChatGPT_1.Engine
{
    public sealed class EngineBootstrap 
    {
        public Database.Database Db { get; }
        public SQLiteWriter Writer { get; }
        public BinaryPipeServer Pipe { get; }

        public EngineBootstrap(string dbPath)
        {
            Db = new Database.Database(dbPath);
            Writer = new SQLiteWriter(Db);
            Pipe = new BinaryPipeServer("DS_PIPE");
        }
        public void Start()
        {
            Db.Init();
            Writer.Start();
            Pipe.Start();
        }
    }
}
