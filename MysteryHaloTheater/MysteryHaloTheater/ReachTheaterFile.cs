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
                if (i > RawData.Length - 2) { break; }

                bool indexFound = false;
                if (
                       (RawData[i] == 0x2B && RawData[i+1] == 0x80 && RawData[i+2] == 0x00)
                    || (RawData[i] == 0x02 && RawData[i + 1] == 0xB8 && RawData[i + 2] == 0x00)
                )
                {
                    int x = i + 2;
                    while (true)
                    {
                        if (x > RawData.Length - 2) { break; }

                        if (RawData[x] > 0)
                        {
                            SegmentStartIndex = x - 3;
                            indexFound = true;
                            break;
                        }
                        x++;
                    }
                    
                }

                if (indexFound)
                {
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
                int SegmentDataIndex = CurrentSegmentIndex + 4;

                Segment seg = new Segment();

                int EOF = BitConverter.ToInt32(RawData, SegmentDataIndex);

                seg.EOFFlag = "";
                if (RawData[SegmentDataIndex] != 0)
                {
                    seg.EOFFlag = System.Text.Encoding.ASCII.GetString(RawData, SegmentDataIndex, 3);
                }

                short SegmentSize;
                if (seg.EOFFlag == "eof") // "eof "
                {
                    SegmentSize = RawData[CurrentSegmentIndex + 3];
                } else {
                    SegmentSize = BitConverter.ToInt16(RawData, CurrentSegmentIndex + 3);
                }
                if (SegmentSize > 1000)
                {
                    break;
                }

                seg.SegmentSize = SegmentSize;

                byte[] tickData = new byte[4];
                Array.Copy(RawData, SegmentDataIndex + 3, tickData, 0, 4);
                Array.Reverse(tickData);
                seg.Tick = BitConverter.ToInt32(tickData, 0);

                seg.Data = new List<byte>();
                for (int i = 0; i < SegmentSize + 4; i++)
                {
                    seg.Data.Add(RawData[CurrentSegmentIndex + i]);
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
        public short SegmentSize { get; set; }
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
