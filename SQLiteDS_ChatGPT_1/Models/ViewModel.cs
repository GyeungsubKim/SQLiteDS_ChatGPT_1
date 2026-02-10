using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Engine;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SQLiteDS_ChatGPT_1.Models
{
    public class ViewModel : INotifyPropertyChanged
    {
        public static ObservableCollection<string> LogText { get; } = [];
        private readonly object _logLock = new();
        public ViewModel(FeedBus bus)
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(LogText, _logLock);
            bus.OnCounter += Bus_OnCounter;
        }
        public static void AddLog(string logText)
        {
            try
            {
                LogText.Add(logText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLog Error: {ex.Message}");
            }
        }
        private string _totalCount = string.Empty;
        public string TotalCount
        {
            get => string.IsNullOrEmpty(_totalCount) ? "0" : _totalCount;
            set
            {
                if (_totalCount != value)
                {
                    long total = (int.Parse(ExpectFut.Replace(",", "")) +
                        int.Parse(CurrentFut.Replace(",", "")) + int.Parse(BidNask.Replace(",", "")) +
                        int.Parse(CurrentOpt.Replace(",", "")) + int.Parse(Trading.Replace(",", "")) +
                        int.Parse(Kospi.Replace(",", "")) + int.Parse(Kos200.Replace(",", "")));
                    string str = $"{total:N0}";
                    _totalCount = str;
                    OnPropertyChanged();
                }
            }
        }
        private string _expectFut = string.Empty;
        public string ExpectFut
        {
            get => string.IsNullOrEmpty(_expectFut) ? "0" : _expectFut;
            set => UpdateValue(ref _expectFut, value, nameof(ExpectFut), ref _expectFutBg, nameof(ExpectFutBg));
        }
        private Brush _expectFutBg = Brushes.Transparent;
        public Brush ExpectFutBg { get => _expectFutBg; }
        private string _currentFut = string.Empty;
        public string CurrentFut
        {
            get => string.IsNullOrEmpty(_currentFut) ? "0" : _currentFut;
            set => UpdateValue(ref _currentFut, value, nameof(CurrentFut), ref _currentFutBg, nameof(CurrentFutBg));
        }
        private Brush _currentFutBg = Brushes.Transparent;
        public Brush CurrentFutBg { get => _currentFutBg; }
        private string _bidNask = string.Empty;
        public string BidNask
        {
            get => string.IsNullOrEmpty(_bidNask) ? "0" : _bidNask;
            set => UpdateValue(ref _bidNask, value, nameof(BidNask), ref _bidNaskBg, nameof(BidNaskBg));
        }
        private Brush _bidNaskBg = Brushes.Transparent;
        public Brush BidNaskBg { get => _bidNaskBg; }
        private string _currentOpt = string.Empty;
        public string CurrentOpt
        {
            get => string.IsNullOrEmpty(_currentOpt) ? "0" : _currentOpt;
            set => UpdateValue(ref _currentOpt, value, nameof(CurrentOpt), ref _currentOptBg, nameof(CurrentOptBg));
        }
        private Brush _currentOptBg = Brushes.Transparent;
        public Brush CurrentOptBg { get => _currentOptBg; }
        private string _trading = string.Empty;
        public string Trading
        {
            get => string.IsNullOrEmpty(_trading) ? "0" : _trading;
            set => UpdateValue(ref _trading, value, nameof(Trading), ref _tradingBg, nameof(TradingBg));
        }
        private Brush _tradingBg = Brushes.Transparent;
        public Brush TradingBg { get => _tradingBg; }
        private string _kospi = string.Empty;
        public string Kospi
        {
            get => string.IsNullOrEmpty(_kospi) ? "0" : _kospi;
            set => UpdateValue(ref _kospi, value, nameof(Kospi), ref _kospiBg, nameof(KospiBg));
        }
        private Brush _kospiBg = Brushes.Transparent;
        public Brush KospiBg { get => _kospiBg; }
        private string _kos200 = string.Empty;
        public string Kos200
        {
            get => string.IsNullOrEmpty(_kos200) ? "0" : _kos200;
            set => UpdateValue(ref _kos200, value, nameof(Kos200), ref _kos200Bg, nameof(Kos200Bg));
        }
        private Brush _kos200Bg = Brushes.Transparent;
        public Brush Kos200Bg { get => _kos200Bg; }
        private void Bus_OnCounter((WorkType, string) tuple)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                WorkType type = tuple.Item1;
                string index = tuple.Item2;
                switch (type)
                {
                    case WorkType.OptionCur:
                        CurrentOpt = Increase(CurrentOpt);
                        break;
                    case WorkType.BidsAsks:
                        BidNask = Increase(BidNask);
                        break;
                    case WorkType.FutureCur:
                        CurrentFut = Increase(CurrentFut);//(int.Parse(CurrentFut.Replace(",", "")) + 1).ToString();
                        break;
                    case WorkType.Ksp:
                        switch (index)
                        {
                            case "1":
                            case "2":
                                Kospi = Increase(Kospi);
                                break;
                                case "3":
                                Kos200 = Increase(Kos200);
                                break;
                            case "4":
                                ExpectFut = Increase(ExpectFut);//(int.Parse(ExpectFut.Replace(",", "")) + 1).ToString();
                                break;
                        }
                        break;
                    case WorkType.Trade:
                        Trading = Increase(Trading);// (int.Parse(Trading.Replace(",", "")) + 1).ToString();
                        break;
                    default:
                            break;
                }
            });
        }
        private string Increase(string value)
        {
            //var property = this.GetType().GetProperty(name);
            //string value = property?.GetValue(this)?.ToString() ?? "0";
            value = value.Replace(",", "");
            int v = int.Parse(value);
            string str = (v + 1).ToString();
            return str;
        }
            //=> (int.Parse(nameof(name).Replace(",", "")) + 1).ToString();
        private bool _isNotTrading = true;
        private void UpdateValue<T>(ref T field, T value, string propertyName, ref Brush bgField, string bgPropertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                ResetAllBackgrounds();
                field = value;
                bgField = Brushes.Cyan;

                OnPropertyChanged(propertyName);
                OnPropertyChanged(bgPropertyName);

                TotalCount = "0";
            }
        }
        private void ResetAllBackgrounds()
        {
            _expectFutBg = Brushes.Transparent;
            _currentFutBg = Brushes.Transparent;
            _bidNaskBg = Brushes.Transparent;
            _currentOptBg = Brushes.Transparent;
            _tradingBg = Brushes.Transparent;
            _kospiBg = Brushes.Transparent;
            _kos200Bg = Brushes.Transparent;

            OnPropertyChanged(nameof(ExpectFutBg));
            OnPropertyChanged(nameof(CurrentFutBg));
            OnPropertyChanged(nameof(BidNaskBg));
            OnPropertyChanged(nameof(CurrentOptBg));
            OnPropertyChanged(nameof(TradingBg));
            OnPropertyChanged(nameof(KospiBg));
            OnPropertyChanged(nameof(Kos200Bg));
        }
        public bool IsNotTrading
        {
            get => _isNotTrading;
            set
            {
                if (_isNotTrading != value)
                {
                    _isNotTrading = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _txtStatus = string.Empty;
        public string TxtStatus
        {
            get => _txtStatus;
            set
            {
                if (_trading != value)
                {
                    _txtStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
