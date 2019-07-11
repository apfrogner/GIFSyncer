using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIFSyncer1
{
    public partial class Form1 : Form
    {
        private Image gif;

        private List<long> Poss;

        private byte[] buf;
        private MemoryStream ms = new MemoryStream();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buf = new byte[3];
            byte[] searchBytes = new byte[3];
            searchBytes[0] = 33;
            searchBytes[1] = 249;
            searchBytes[2] = 4;
            Poss = new List<long>();
            int flicker = 0;

            var t = new OpenFileDialog();

            if (t.ShowDialog() == DialogResult.OK)
            {
                //gif = Image.FromFile(t.FileName);
                byte[] img = File.ReadAllBytes(t.FileName);
                ms = new MemoryStream(img);
                var start = DateTime.Now;
                Console.WriteLine("Start");
                while (ms.Position != ms.Length)
                {
                    var bt = ms.ReadByte();

                    buf[flicker] = (byte)bt;

                    if (flicker == 2)
                    {
                        if (ArraysMatch(searchBytes, buf, 0))
                        {
                            Poss.Add(ms.Position - searchBytes.Length);
                        }
                        else
                        {
                            ms.Position = ms.Position - (searchBytes.Length - 1);
                        }
                        flicker = 0;
                        //flicker = 1;
                    }
                    else
                    {
                        flicker++;
                    }

                }
                var end = DateTime.Now;

                Console.WriteLine("Done");
                Console.WriteLine((end - start).TotalMilliseconds.ToString());
                //var af = ms.ScanUntilFound(buf);
                //MessageBox.Show(af.ToString());
            }


            adjustGifSpeed(100);
            trackBar1.Value = 100;
            label1.Text = (((60000/(trackBar1.Value*10.0)))).ToString();

            label2.Text = trackBar1.Value.ToString();
        }

        private void adjustGifSpeed(int delay)
        {
            BinaryWriter bw = new BinaryWriter(ms);
            
            foreach (var pos in Poss)
            {
                bw.Seek(Convert.ToInt32(pos + 4), 0);
                    
                bw.Write(delay);
            }

            bw.Flush();

            var str = ms.Length.ToString();

            if (ms.Length > 0)
            {
                pictureBox1.Image = Image.FromStream(ms);
            }

        }

        bool ArraysMatch(byte[] searchBytes, byte[] streamBuffer, int startAt)
        {
            for (int i = 0; i < searchBytes.Length - startAt; i++)
            {
                if (searchBytes[i] != streamBuffer[i + startAt])
                    return false;
            }
            return true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            adjustGifSpeed(trackBar1.Value);

            
            label1.Text = (((60000/(trackBar1.Value*10.0)))).ToString();

            label2.Text = trackBar1.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ms.Length > 0)
            {
                pictureBox1.Image = Image.FromStream(ms);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var img = Image.FromStream(ms);

            img.Save(Guid.NewGuid().ToString() + ".gif", ImageFormat.Gif);
        }

    }
}
