using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using NCrontab;

namespace BackgroundChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> _pictures = new List<string>();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;

        private DateTime NextImageShift { get; set; } = DateTime.Now;
        private string Cron { get; set; } = "* * * * *";
        public MainWindow()
        {
            InitializeComponent();
            Thread thread = new Thread(new ThreadStart(SetPictureBackground));
            thread.Start();


            Dictionary<string, string> cronDic = new Dictionary<string, string>();
            cronDic.Add("Every Minute", "* * * * *");
            cronDic.Add("Every Half an hour", "0,30 * * * *");
            cronDic.Add("Every Hour", "0 * * * *");
            cronDic.Add("Every 6 hour", "0 6,12,18,00 * * *");
            cronDic.Add("Every 12 hour", "0 12,00 * * *");
            cronDic.Add("Every day", "0 00 * * *");
            cronDic.Add("Every week", "0 * * * 1");
            cronDic.Add("Every Fortnight", "0 * 1,14 * *");
            cronDic.Add("Every Month", "0 * 1 * *");

           
            ScheduleSelecter.ItemsSource = cronDic;
            ScheduleSelecter.DisplayMemberPath = "Key";
            ScheduleSelecter.SelectedValuePath = "Value";
            ImageList.DataContext = _pictures;

        }

        private void SelectPictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*",
                InitialDirectory = "C:\\Pictures"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    _pictures.Add(Path.GetFullPath(filename));
                }

            }
            ImageList.ItemsSource = _pictures;
        }
        //Look into better ways to change image than using user32
        private static void DisplayPicture(string file_name)
        {
            uint flags = 0;
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0, file_name, flags))
            {
                Console.WriteLine("Error");
            }
        }

        private void SetScheduledTime()
        {

            var schedule = CrontabSchedule.Parse(Cron);
            NextImageShift = schedule.GetNextOccurrence(DateTime.Now);
        }

        private void SetPictureBackground()
        {
            while (true)
            {
                if (_pictures.Count == 0)
                {

                }
                else
                {
                    if (DateTime.Now > NextImageShift)
                    {
                        Random r = new Random();
                        int pathIndex = r.Next(0, _pictures.Count);
                        DisplayPicture(_pictures[pathIndex]);
                        SetScheduledTime();
                    }
                }
            }
        }

        private void ScheduleSelecter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Cron = ScheduleSelecter.SelectedValue.ToString();
            SetScheduledTime();
        }
    }
}
