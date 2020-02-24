using Microsoft.Win32;
using System;
using System.Collections;
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
            if (!IsInitialized)
                return;

            lv_TheaterData.Items.Clear();
            lv_Players.Items.Clear();

            lbl_AuthorA.Content = WorkingFile?.CreatorNameA ?? "";
            lbl_MissionName.Content = WorkingFile?.MissionName ?? "";
            lbl_MissionDescription.Content = WorkingFile?.MissionFullDescription ?? "";
            lbl_MissionTime.Content = "";
        }

        private void ReloadFileDisplay()
        {
            ClearDisplay();
            
            if (WorkingFile == null)
            {
                return;
            }

            float missionTimeActual = (float)WorkingFile.TickCount / 60.0f;
            TimeSpan missionTimeSpan = TimeSpan.FromSeconds(missionTimeActual);

            string missionTimeStr = $"Theater Time: {missionTimeSpan.ToString(@"hh\:mm\:ss")}"+
                $" (Actual: {missionTimeSpan.ToString(@"hh\:mm\:ss\.fff")} / Ticks: {WorkingFile.TickCount})";

            lbl_MissionTime.Content = missionTimeStr;

            foreach (var segment in WorkingFile.TheaterSegments)
            {
                int DataStartIndex = (chk_HideSizeTick.IsChecked ?? false) ? 11 : 0;
                StringBuilder hexBytes = new StringBuilder();

                int segmentHexCounter = 0;
                for (int i = DataStartIndex; i < segment.Data.Count; i++)
                {
                    if (segmentHexCounter > 0 && segmentHexCounter % 2 == 0)
                    {
                        hexBytes.Append(" ");
                    }
                    if (cbx_ShowDataAsBits.IsChecked == true)
                    {
                        hexBytes.Append(Convert.ToString(segment.Data[i], 2).PadLeft(8, '0'));
                    } else {
                        hexBytes.AppendFormat("{0:X02}", segment.Data[i]);
                    }
                    
                    segmentHexCounter++;
                }

                TimeSpan timeActualSpan = TimeSpan.FromSeconds((float)segment.Tick / 60.0f);

                lv_TheaterData.Items.Add(new {
                    Size = segment.SegmentSize,
                    EOF = segment.EOFFlag,
                    Tick = segment.Tick,
                    Time = timeActualSpan.ToString(@"hh\:mm\:ss\.fff"),
                    Data = hexBytes.ToString()
                });
            }

            foreach(var player in WorkingFile.Players)
            {
                lv_Players.Items.Add(player);
            }

        }

        public void ReverseBitArray(BitArray array)
        {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++)
            {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }
        }

        private void lv_TheaterData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkingFile == null)
                return;
            if (lv_TheaterData.Items.Count <= 0)
                return;
            if (lv_TheaterData.SelectedIndex < 0)
                return;

            int idx = lv_TheaterData.SelectedIndex;
            Segment seg = WorkingFile.TheaterSegments[idx];

            lbl_Info_Tick_Value.Content = seg.Tick.ToString();
            lbl_Info_SegmentSize_Value.Content = seg.SegmentSize.ToString();

            int horizontalRotationStartBit = 191;
            int verticalRotationStartBit = 204;

            StringBuilder hexBytes = new StringBuilder();
            for (int i = 0; i < seg.Data.Count; i++)
            {
                hexBytes.Append(Convert.ToString(seg.Data[i], 2).PadLeft(8, '0'));
            }

            tb_Info_SegmentBits.Text = hexBytes.ToString();

            string horizontalRotationBitString = hexBytes.ToString().Substring(horizontalRotationStartBit, 13);
            string verticalRotationBitString = hexBytes.ToString().Substring(verticalRotationStartBit, 12);

            int horizontalValue = Convert.ToInt32(horizontalRotationBitString, 2);
            int verticalValue = Convert.ToInt32(verticalRotationBitString, 2);

            PlayerHorizontalRotation phr = new PlayerHorizontalRotation(horizontalValue);
            PlayerVerticalRotation pvr = new PlayerVerticalRotation(verticalValue);

            lbl_Info_PlayerHorizontalRotation.Content = horizontalRotationBitString + " " + horizontalValue.ToString("X4") + " " + phr.ToDegrees();
            lbl_Info_PlayerVerticalRotation.Content = verticalRotationBitString + " " + verticalValue.ToString("X3") + " " + pvr.ToDegrees();

        }

        private void chk_HideSizeTick_Checked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }
        private void chk_HideSizeTick_Unchecked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }
        private void cbx_ShowDataAsBits_Unchecked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }
        private void cbx_ShowDataAsBits_Checked(object sender, RoutedEventArgs e)
        {
            ReloadFileDisplay();
        }

        private void btn_Info_DissectBits_Click(object sender, RoutedEventArgs e)
        {
            if (WorkingFile == null)
                return;
            if (lv_TheaterData.Items.Count <= 0)
                return;
            if (lv_TheaterData.SelectedIndex < 0)
                return;

            int idx = lv_TheaterData.SelectedIndex;
            Segment seg = WorkingFile.TheaterSegments[idx];

            DissectBitsWindow dbWindow = new DissectBitsWindow(seg.Data);

            dbWindow.Show();
        }
    }
}
