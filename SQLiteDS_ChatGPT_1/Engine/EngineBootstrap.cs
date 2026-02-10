using SQLiteDS_ChatGPT_1.Database;

namespace SQLiteDS_ChatGPT_1.Engine
{
    public sealed class EngineBootstrap 
    {
        public FeedBus Bus { get; }
        public EngineBootstrap(string dbPath)
        {
            var db = new Database.Database(dbPath);
            db.Init();
            db.Start();

            var pipe = new BinaryPipeServer();
            pipe.Start("DS_PIPE");

            Bus = new FeedBus(db, pipe);
            Bus.Start();
        }
    }
}
