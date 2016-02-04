using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Trixel_Creator
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.CheckFileExists)
            {
                this.Activate();
                FileStream fileStream = new FileInfo(openFileDialog1.FileName).OpenRead();
                previewPictureBox.Image = Bitmap.FromStream(fileStream);
                fileStream.Close();
                trixelButton.Enabled = true;
                trackBar1.Enabled = true;
                trackBar1.Minimum = 1;
                trackBar1.Maximum = previewPictureBox.Image.Width > previewPictureBox.Image.Height ? previewPictureBox.Image.Width : previewPictureBox.Image.Height;
                sizeLabel.Text = "" + trackBar1.Value;
            }

        }
        private Color averageCol(List<Color> colours)
        {
            if (colours.Count > 0)
            {
                int red = 0;
                int green = 0;
                int blue = 0;
                foreach (Color c in colours)
                {
                    red += c.R;
                    green += c.G;
                    blue += c.B;
                }
                return Color.FromArgb(red / colours.Count, green / colours.Count, blue / colours.Count);
            } else
            {
                return Color.FromArgb(0, 0, 0);
            }

        }
        private void fileButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
        private void trixelButton_Click(object sender, EventArgs e)
        {
            Bitmap myBitmap = new Bitmap(previewPictureBox.Image);
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            Tuple<Bitmap, int> indata = new Tuple<Bitmap, int>(myBitmap, trackBar1.Value);
            backgroundWorker1.RunWorkerAsync(indata);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                previewPictureBox.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                fs.Close();
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            sizeLabel.Text = ""+trackBar1.Value;
        }

        private void statusStrip1_Resize(object sender, EventArgs e)
        {
            progressBar1.Width = statusStrip1.Width - 20;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Tuple<Bitmap, int> indata = (Tuple < Bitmap, int>)e.Argument;
            Bitmap myBitmap = indata.Item1;
            List<Color> colours1 = new List<Color>();
            List<Color> colours2 = new List<Color>();
            int maximum = (myBitmap.Width * myBitmap.Height) * 2;
            int done = 0;
            int progress = 0;
            int size = indata.Item2;
            for (int i = 0; i < myBitmap.Width; i += size)
            {
                for (int j = 0; j < myBitmap.Height; j += size)
                {
                    for (int Xcount = 0; Xcount < size && i + Xcount < myBitmap.Width; Xcount++)
                    {
                        for (int Ycount = 0; Ycount < size && j + Ycount < myBitmap.Height; Ycount++)
                        {
                            done++;
                            if ((int)Math.Round((double)(100 * done) / maximum) > progress)
                            {
                                progress = (int)Math.Round((double)(100 * done) / maximum);
                                worker.ReportProgress(progress,new Bitmap(myBitmap));
                            }
                            if (Ycount < size - Xcount)
                            {
                                colours1.Add(myBitmap.GetPixel(i + Xcount, j + Ycount));
                            }
                            else
                            {
                                colours2.Add(myBitmap.GetPixel(i + Xcount, j + Ycount));
                            }
                        }
                    }

                    Color avg1 = averageCol(colours1);
                    Color avg2 = averageCol(colours2);
                    colours1 = new List<Color>();
                    colours2 = new List<Color>();
                    for (int Xcount = 0; Xcount < size && i + Xcount < myBitmap.Width; Xcount++)
                    {
                        for (int Ycount = 0; Ycount < size && j + Ycount < myBitmap.Height; Ycount++)
                        {
                            done++;
                            if ((int)Math.Round((double)(100 * done) / maximum) > progress)
                            {
                                progress = (int)Math.Round((double)(100 * done) / maximum);
                                worker.ReportProgress(progress, new Bitmap(myBitmap));
                            }
                            if (Ycount < size - Xcount)
                            {
                                myBitmap.SetPixel(i + Xcount, j + Ycount, avg1);
                            }
                            else
                            {
                                myBitmap.SetPixel(i + Xcount, j + Ycount, avg2);
                            }
                        }
                    }

                }
            }
            e.Result = myBitmap;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            previewPictureBox.Image = (Bitmap)e.Result;
            saveButton.Enabled = true;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            previewPictureBox.Image = (Bitmap)e.UserState;
            progressBar1.Value = e.ProgressPercentage;
        }
    }
}
