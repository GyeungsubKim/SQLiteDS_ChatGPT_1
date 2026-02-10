using SQLiteDS_ChatGPT_1.Core;
using SQLiteDS_ChatGPT_1.Daishin;
using SQLiteDS_ChatGPT_1.Engine;
using SQLiteDS_ChatGPT_1.Models;
using System.IO;
using System.Windows;

namespace SQLiteDS_ChatGPT_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel? _vm;
        public static Works? work = null;

        public static string CurPath = Directory.GetCurrentDirectory();
        public static string CurDate = DateTime.Now.ToString("yyyyMMdd");
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }
        string dataPath = Path.Combine(CurPath, "Datas");
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            ViewModel.AddLog($"데이터 베이스 저장 장소 : {dataPath}");
            
            var dbPah = Path.Combine(dataPath, $"sql_{CurDate}.db3");
            var engine = new EngineBootstrap(dbPah);
            work = new Works(engine.Bus);
            _vm = new ViewModel(engine.Bus);
            
            DataContext = _vm;
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => work?.Run());
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            work = null;
            _vm = null;

            base.Close();
        }
    }
}