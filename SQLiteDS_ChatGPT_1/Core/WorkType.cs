using SQLiteDS_ChatGPT_1.Models;

namespace SQLiteDS_ChatGPT_1.Core
{
    public enum WorkType
    {
        [TableInfo("tblCodes", typeof(FutureCode))] FutureCode = 0,
        [TableInfo("tblOptionCodes", typeof(OptionCode))] OptionCode = 1,
        [TableInfo("tblHighLow", typeof(HighLow))] HighLow = 2,
        [TableInfo("tblTrd", typeof(Trading))] Trade = 3,
        [TableInfo("tblMst", typeof(Master))] Master = 4,
        [TableInfo("tblCur", typeof(FutureCur))] FutureCur = 5,
        [TableInfo("tblOpt", typeof(OptionCur))] OptionCur = 6,
        [TableInfo("tblBid", typeof(BidsAsks))] BidsAsks = 7,
        [TableInfo("tblKsp", typeof(KSP))] Ksp = 8,
        [TableInfo("tblWork", typeof(WorkLog))] Works = 9
    }
}
