using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MysteryHaloTheater
{
    /// <summary>
    /// Interaction logic for DissectBitsWindow.xaml
    /// </summary>
    public partial class DissectBitsWindow : Window
    {
        private List<byte> bits;

        public DissectBitsWindow(List<byte> bitArray)
        {
            InitializeComponent();

            bits = bitArray;

            StringBuilder hexBytes = new StringBuilder();
            for (int i = 0; i < bits.Count; i++)
            {
                hexBytes.Append(Convert.ToString(bits[i], 2).PadLeft(8, '0'));
            }

            tblk_Bits.Text = hexBytes.ToString();
        }
    }
}
