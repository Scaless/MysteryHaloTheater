using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MysteryHaloTheater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReachTheaterFile WorkingFile;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_LoadTheaterFile_Click(object sender, RoutedEventArgs e)
        {
            WorkingFile = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\MCC\Temporary\UserContent\HaloReach\Movie";
            dialog.Filter = "Reach Theater Files (*.mov;*.film)|*.mov;*.film|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (File.Exists(dialog.FileName))
                {
                    WorkingFile = new ReachTheaterFile(dialog.FileName);
                }
            }

            ReloadFileDisplay();
        }

        private void ClearDisplay()
        {
            lv_TheaterData.Items.Clear();
            lv_Players.Items.Clear();

            lbl_AuthorA.Content = WorkingFile?.CreatorNameA ?? "";
            lbl_MissionName.Content = WorkingFile?.MissionName ?? "";
            lbl_MissionDescription.Content = WorkingFile?.MissionFullDescription ?? "";
        }

        private void ReloadFileDisplay()
        {
            ClearDisplay();
            
            if (WorkingFile == null)
            {
                return;
            }

            foreach (var segment in WorkingFile.TheaterSegments)
            {
                int DataStartIndex = (chk_HideSizeTick.IsChecked ?? false) ? 11 : 0;
                StringBuilder hexBytes = new StringBuilder();

                int segmentHexCounter = 0;
                for (int i = DataStartIndex; i < segment.Data.Count; i++)
                {
                    if (segmentHexCounter > 0 && segmentHexCounter % 4 == 0)
                    {
                        hexBytes.Append(" ");
                    }
                    hexBytes.AppendFormat("{0:X02}", segment.Data[i]);
                    segmentHexCounter++;
                }

                lv_TheaterData.Items.Add(new {
                    Size = segment.SegmentSize,
                    EOF = segment.EOFFlag,
                    Tick = segment.Tick,
                    Data = hexBytes.ToString()
                });
            }

            foreach(var player in WorkingFile.Players)
            {
                lv_Players.Items.Add(player);
            }

        }

        private void chk_HideSizeTick_Checked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }

        private void chk_HideSizeTick_Unchecked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }
    }
}
