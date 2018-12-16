using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace AudioNormalizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Settings.app = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("app.json"));

            AudioOutName.Text = Settings.app.AudioOutName.ToString();
            AudioCells.Text = Settings.app.AudioCells.ToString();
            UpTime.Text = Settings.app.UpTime.ToString();
            InitVol.Text = Settings.app.InitVol.ToString();
            MinVol.Text = Settings.app.MinVol.ToString();
            MaxPeakVol.Text = Settings.app.MaxPeakVol.ToString();
            AvgVol.Text = Settings.app.AvgVol.ToString();
        }


        Thread thNormalizer;
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (btn_StartOrStop.Content?.ToString() == "Start")
            {
                thNormalizer = new Thread(() => Normalizer.MonitorPeakValue());
                thNormalizer.Start();

                btn_StartOrStop.Content = "Stop";
            }
            else
            {
                thNormalizer.Abort();
                btn_StartOrStop.Content = "Start";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.app.AudioOutName = AudioOutName.Text;
            Settings.app.AudioCells = int.Parse(AudioCells.Text);
            Settings.app.UpTime = int.Parse(UpTime.Text);
            Settings.app.InitVol = float.Parse(InitVol.Text.Replace(".", ","));
            Settings.app.MinVol = float.Parse(MinVol.Text.Replace(".", ","));
            Settings.app.MaxPeakVol = float.Parse(MaxPeakVol.Text.Replace(".", ","));
            Settings.app.AvgVol = float.Parse(AvgVol.Text.Replace(".", ","));

            File.WriteAllText("app.json", JsonConvert.SerializeObject(Settings.app));
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thNormalizer?.Abort();
        }
    }
}
