using System.IO;
using System.Reflection;
using System.Text;

namespace SQLiteDS_ChatGPT_1.Core
{
    #region Attributes

    [AttributeUsage(AttributeTargets.Field)]
    public class TableInfoAttribute(string tableName, Type modelType) : Attribute
    {
        public string TableName { get; } = tableName;
        public Type ModelType { get; } = modelType;
    }

    #endregion


    #region Model Classes

    public interface IBinarySerializable
    {
        void WriteTo(BinaryWriter writer);
    }

    public class Basic : IBinarySerializable
    {
        public long Idx { get; set; }
        public string? Code { get; set; }
        public override string ToString() => $"{Idx} {Code}";

        public virtual void WriteTo(BinaryWriter writer)
        {
            writer.Write(Code!);
        }
    }

    public class WorkLog : IBinarySerializable 
    {
        public long Idx { get; set; }
        public DateTime Time { get; set; }
        public int TableId { get; set; }
        public long ForeignKey { get; set; }
        public long Seq { get; set; }
        public override string ToString() => $"{Idx} {Time} {TableId} {ForeignKey} {Seq}";
        
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Time.ToBinary());
            writer.Write(TableId);
            writer.Write(ForeignKey);
            writer.Write(Seq);
        }
    }

    public class FutureCode : Basic, IBinarySerializable
    {
        public string? Name { get; set; }
        public override string ToString() => $"{base.ToString()} {Name}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Name!);
        }
    }

    public class OptionCode : FutureCode
    {
        public string? Classify { get; set; }
        public string? Month { get; set; }
        public decimal DoPrice { get; set; }
        public override string ToString() => $"{base.ToString()} {Classify} {Month} {DoPrice:0.00}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Classify!);
            writer.Write(Month!);
            writer.Write(DoPrice!);
        }
    }

    public class KSP : Basic
    {
        public long Time { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Price} {Change}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time!);
            writer.Write(Price!);
            writer.Write(Change!);
        }
    }

    public class Price : Basic
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public long Volume { get; set; }
        public decimal Close { get; set; }
        public long OpenInterest { get; set; }
        public decimal HL => High - Low;
        public decimal Body => Close - Open;
        public decimal UpTail => High - Math.Max(Open, Close);
        public decimal DnTail => Math.Min(Open, Close) - Low;
        public override string ToString() => $"{base.ToString()} {Open} {High} {Low} {Close} {Volume:#,###} {OpenInterest:#,###}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Open);
            writer.Write(High);
            writer.Write(Low);
            writer.Write(Volume);
            writer.Write(Close);
            writer.Write(OpenInterest);
        }
    }

    public class Master : Price
    {
        public long Time { get; set; }
        public decimal Change { get; set; }
        public long PrevOpenInterest { get; set; }
        public long Turnover { get; set; }
        public long SellQuoteCount { get; set; }
        public long BuyQuoteCount { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Change} {PrevOpenInterest} {Turnover} {SellQuoteCount} {BuyQuoteCount}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Change);
            writer.Write(PrevOpenInterest);
            writer.Write(Turnover);
            writer.Write(SellQuoteCount);
            writer.Write(BuyQuoteCount);    
        }
    }

    public class HighLow : Price
    {
        public long Date { get; set; }
        public override string ToString() => $"{base.ToString()} {Date}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Date);
        }
    }

    public class Current : Price
    {
        public long Time { get; set; }
        public decimal Change { get; set; }
        public long Turnover { get; set; }
        public long TradeType { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Change} {Turnover} {TradeType}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(Change);
            writer.Write(Turnover);
            writer.Write(TradeType);
        }   
    }

    public class FutureCur : Current
    {
        public long SellVolume { get; set; }
        public long BuyVolume { get; set; }
        public decimal Basis { get; set; }
        public decimal K200 { get; set; }
        public override string ToString() => $"{base.ToString()} {SellVolume} {BuyVolume} {Basis} {K200}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(SellVolume);
            writer.Write(BuyVolume);
            writer.Write(Basis);
            writer.Write(K200);
        }
    }

    public class BidsAsks : Basic
    {
        public long Time { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidPrice { get; set; }
        public long AskTotalQuantity { get; set; }
        public long BidTotalQuantity { get; set; }
        public byte MarketState { get; set; }
        public long Ask1Quantity { get; set; }
        public long Bid1Quantity { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {AskPrice} {BidPrice} {AskTotalQuantity} {BidTotalQuantity} {MarketState} {Ask1Quantity} {Bid1Quantity}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write(AskPrice);
            writer.Write(BidPrice);
            writer.Write(AskTotalQuantity);
            writer.Write(BidTotalQuantity);
            writer.Write(MarketState);
            writer.Write(Ask1Quantity);
            writer.Write(Bid1Quantity);
        }
    }

    public class OptionCur : Current
    {
        public decimal TheoreticalValue { get; set; }
        public decimal IV { get; set; }
        public decimal Delta { get; set; }
        public decimal Gamma { get; set; }
        public decimal Theta { get; set; }
        public decimal Vega { get; set; }
        public decimal Rho { get; set; }
        public decimal BestAsk { get; set; }
        public decimal BestBid { get; set; }
        public override string ToString() => $"{base.ToString()} {TheoreticalValue} {IV} {Delta} {Gamma} {Theta} {Vega} {Rho} {BestAsk} {BestBid}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write((decimal)TheoreticalValue);
            writer.Write((decimal)IV);
            writer.Write((decimal)Delta);
            writer.Write((decimal)Gamma);
            writer.Write((decimal)Theta);
            writer.Write((decimal)Vega);
            writer.Write((decimal)Rho);
            writer.Write((decimal)BestAsk);
            writer.Write((decimal)BestBid);
        }
    }

    public class Trading : Basic
    {
        public long Time { get; set; }
        public int Participant { get; set; }
        public long SellVolume { get; set; }
        public long SellValue { get; set; }
        public long BuyVolume { get; set; }
        public long BuyValue { get; set; }
        public override string ToString() => $"{base.ToString()} {Time} {Participant} {SellVolume} {SellValue} {BuyVolume} {BuyValue}";

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(Time);
            writer.Write((long)Participant);
            writer.Write((long)SellVolume);
            writer.Write((long)SellValue);
            writer.Write((long)BuyVolume);
            writer.Write((long)BuyValue);
        }
    }

    #endregion

    #region Record Count Tracker

    public class RecordCountInfo
    {
        public string TableName { get; set; } = string.Empty;
        public int CurrentCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class RecordCountTracker
    {
        private static readonly RecordCountTracker _instance = new();
        private readonly Dictionary<WorkType, RecordCountInfo> _counts = [];
        private readonly object _lock = new();

        public static RecordCountTracker Instance => _instance;

        private RecordCountTracker()
        {
            InitializeCounts();
        }

        private void InitializeCounts()
        {
            foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
            {
                var field = typeof(WorkType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<TableInfoAttribute>();

                if (attr != null)
                {
                    _counts[type] = new RecordCountInfo
                    {
                        TableName = attr.TableName,
                        CurrentCount = 0,
                        TotalCount = 0,
                        LastUpdated = DateTime.Now
                    };
                }
            }
        }

        public void AddRecords(WorkType workType, int count)
        {
            lock (_lock)
            {
                if (_counts.TryGetValue(workType, out var info))
                {
                    info.CurrentCount += count;
                    info.TotalCount += count;
                    info.LastUpdated = DateTime.Now;
                }
            }
        }

        public void ResetCurrentCount(WorkType workType)
        {
            lock (_lock)
            {
                if (_counts.TryGetValue(workType, out var info))
                {
                    info.CurrentCount = 0;
                    info.LastUpdated = DateTime.Now;
                }
            }
        }

        public RecordCountInfo GetCountInfo(WorkType workType)
        {
            lock (_lock)
            {
                return _counts.TryGetValue(workType, out var info) ? info : new RecordCountInfo();
            }
        }

        public IEnumerable<RecordCountInfo> GetAllCounts()
        {
            lock (_lock)
            {
                return _counts.Values.ToList();
            }
        }

        public void ResetAllCurrentCounts()
        {
            lock (_lock)
            {
                foreach (var info in _counts.Values)
                {
                    info.CurrentCount = 0;
                    info.LastUpdated = DateTime.Now;
                }
            }
        }
    }

    #endregion

    #region SQL Generator

    public static class SqlCreateGenerator
    {
        public static string GenerateAllTablesSql()
        {
            var sb = new StringBuilder();

            foreach (WorkType type in Enum.GetValues(typeof(WorkType)))
            {
                var field = typeof(WorkType).GetField(type.ToString());
                var attr = field?.GetCustomAttribute<TableInfoAttribute>();
                if (attr == null) continue;

                string tableName = attr.TableName;
                var props = attr.ModelType.GetProperties()
                    .Where(p => p.Name != "Idx")
                    .ToList();

                var cols = new List<string>();
                foreach (var p in props)
                {
                    string sqlType = MapType(p.PropertyType);
                    string colName = p.Name;
                    bool notNull = p.PropertyType.IsValueType && Nullable.GetUnderlyingType(p.PropertyType) == null;
                    string nullSpce = notNull ? " NOT NULL" : "";
                    cols.Add($"[{colName}] {sqlType} {nullSpce}".Trim());
                }

                sb.AppendLine($"CREATE TABLE IF NOT EXISTS [{tableName}] (");
                sb.AppendLine("    [Idx] INTEGER PRIMARY KEY AUTOINCREMENT,");
                sb.AppendLine("    " + string.Join(",\n    ", cols));
                sb.AppendLine(");");

                if (tableName.Contains("Code"))
                {
                    sb.AppendLine($"CREATE INDEX IF NOT EXISTS IDX_{tableName}_Code ON [{tableName}] (Code);");
                }

                if (tableName.Equals("tblWork"))
                {
                    sb.AppendLine($"CREATE TRIGGER IF NOT EXISTS trg_Log{tableName} AFTER INSERT ON [{tableName}] ");
                    sb.AppendLine("BEGIN");
                    sb.AppendLine($"    INSERT INTO tblWork (TableId, ForeignKey) ");
                    sb.AppendLine($"    VALUES ({(int)type}, NEW.Idx);");
                    sb.AppendLine("END;");
                }
                sb.AppendLine();
            }

            sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS idxWorkID ON [tblWork] (TableId, ForeignKey);");
            sb.AppendLine("CREATE UNIQUE INDEX IF NOT EXISTS idxHighLow ON [tblHighLow] (Code, Date);");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string MapType(Type t)
        {
            if (t == typeof(DateTime)) return "DATETIME";
            var code = Type.GetTypeCode(t);
            return code switch
            {
                TypeCode.String => "TEXT",
                TypeCode.Int32 or TypeCode.Int64 or TypeCode.Boolean => "INTEGER",
                TypeCode.Decimal or TypeCode.Double => "REAL",
                _ => "TEXT",
            };
        }
    }

    #endregion

}
