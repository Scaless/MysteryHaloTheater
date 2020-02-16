using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MysteryHaloTheater
{
    class ReachTheaterFile
    {
        public string CreatorNameA; // 20 Bytes, ASCII
        public string CreatorNameB; // 20 Bytes, ASCII
        public string MissionName; // 256 Bytes, UTF16
        public string MissionFullDescription; // 256 Bytes, UTF16

        public Player[] Players = new Player[4]; 

        public List<Segment> TheaterSegments = new List<Segment>();

        public ReachTheaterFile(string TheaterFilePath)
        {
            LoadFile(TheaterFilePath);
        }

        private void LoadFile(string TheaterFilePath)
        {
            byte[] RawData = File.ReadAllBytes(TheaterFilePath);

            // Basic info
            CreatorNameA = System.Text.Encoding.ASCII.GetString(RawData, 0x88, 20);
            CreatorNameB = System.Text.Encoding.ASCII.GetString(RawData, 0xAC, 20);
            MissionName = System.Text.Encoding.Unicode.GetString(RawData, 0xC0, 256);
            MissionFullDescription = System.Text.Encoding.Unicode.GetString(RawData, 0x1C0, 256);

            Players[0].Name = System.Text.Encoding.Unicode.GetString(RawData, 0x1E228, 32);
            Players[0].Tag = System.Text.Encoding.Unicode.GetString(RawData, 0x1E26C, 6);

            Players[1].Name = System.Text.Encoding.Unicode.GetString(RawData, 0x1E330, 32);
            Players[1].Tag = System.Text.Encoding.Unicode.GetString(RawData, 0x1E374, 6);

            Players[2].Name = System.Text.Encoding.Unicode.GetString(RawData, 0x1E438, 32);
            Players[2].Tag = System.Text.Encoding.Unicode.GetString(RawData, 0x1E47C, 6);

            Players[3].Name = System.Text.Encoding.Unicode.GetString(RawData, 0x1E540, 32);
            Players[3].Tag = System.Text.Encoding.Unicode.GetString(RawData, 0x1E584, 6);

            int TagStartIndex = 0x1F760;
            int SegmentStartIndex = 0;

            // Get the index for the segment start
            for (int i = TagStartIndex; ; i++)
            {
                if (i > RawData.Length - 2)
                {
                    break;
                }

                if(RawData[i] == 0x2B && RawData[i+1] == 0x80 && RawData[i+2] == 00)
                {
                    SegmentStartIndex = i + 3;
                    break;
                }
            }

            // Error: Failed to find segment start
            if (SegmentStartIndex == 0)
            {
                return;
            }

            int CurrentSegmentIndex = SegmentStartIndex;
            while (true)
            {
                byte SegmentSize = RawData[CurrentSegmentIndex + 3];
                int SegmentDataIndex = CurrentSegmentIndex + 4;

                int EOF = BitConverter.ToInt32(RawData, SegmentDataIndex);
                

                Segment seg = new Segment();
                seg.SegmentSize = SegmentSize;

                seg.EOFFlag = "";
                if(RawData[SegmentDataIndex] != 0)
                {
                    seg.EOFFlag = System.Text.Encoding.ASCII.GetString(RawData, SegmentDataIndex, 3);
                }

                byte[] tickData = new byte[4];
                Array.Copy(RawData, SegmentDataIndex + 3, tickData, 0, 4);
                Array.Reverse(tickData);
                seg.Tick = BitConverter.ToInt32(tickData, 0);

                seg.Data = new List<byte>();
                for (int i = 0; i < SegmentSize; i++)
                {
                    seg.Data.Add(RawData[SegmentDataIndex + i]);
                }

                TheaterSegments.Add(seg);

                if (seg.EOFFlag == "eof") // "eof "
                {
                    break;
                }

                // Move to the next segment
                CurrentSegmentIndex = SegmentDataIndex + SegmentSize;
            }

        }
    }

    struct Segment
    {
        public byte SegmentSize { get; set; }
        public List<byte> Data { get; set; }
        public string EOFFlag { get; set; }
        public int Tick { get; set; }
    }

    struct Player
    {
        public string Name { get; set; }
        public string Tag { get; set; }
    }

}
