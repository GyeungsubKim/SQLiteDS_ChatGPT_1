using SQLiteDS_ChatGPT_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDS_ChatGPT_1.Daishin
{
    public class Daishin : IDisposable
    {
        private readonly CybosConnection _connection;
        public DateTime _workDate, _currLimit, _postLimit;
        public DateTime _openTime, _closeTime;

        public Daishin()
        {
            _connection ??= new();

            (_openTime, _closeTime) = _connection.GetTime();
            LimitDate();
        }
        private void LimitDate()
        {
            _workDate = DateTime.Now;
            //_curDate = _workDate.ToString("yyyyMMdd");
            _currLimit = SeconThursDay(DateTime.Now, true);
            if (_currLimit.Month < 3)
            {
                DateTime sd = new(_currLimit.Year - 1, _currLimit.Month + 12 - 2, 1);
                _postLimit = SeconThursDay(sd, false);
            }
            else
                _postLimit = SeconThursDay(new DateTime(_currLimit.Year, _currLimit.Month, 1), false);
        }
        private static DateTime SeconThursDay(DateTime now, bool v)
        {
            DateTime sd;
            int i;// = 0;
            DayOfWeek dw = new DateTime(now.Year, now.Month, 1).DayOfWeek;
            if (dw == DayOfWeek.Thursday)
                i = 1;
            else if (dw > DayOfWeek.Thursday)
                i = 8 - ((int)dw - (int)DayOfWeek.Thursday);
            else
                i = 1 + (int)DayOfWeek.Thursday - (int)dw;
            sd = new DateTime(now.Year, now.Month, i + 7);

            if (v & sd.Date < now.Date)
            {
                if (sd.Month == 12)
                {
                    sd = new DateTime(sd.Year + 1, 1, 1);
                    return SeconThursDay(sd, false);
                }
                else
                    return SeconThursDay(new DateTime(sd.Year, sd.Month + 1, 1), false);
            }
            return sd;
        }
        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
}
