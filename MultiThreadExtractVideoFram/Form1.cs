using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using Accord.Video.FFMPEG;

namespace MultiThreadExtractVideoFram
{
    public partial class Form1 : Form
    {
        Double TotalFrame;
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        #region methods

        public void SelectFile()
        {
            OpenFileDialog loadVideoDialog = new OpenFileDialog();
            loadVideoDialog.Filter = "Video files (*.avi, *.mkv, *.mp4) | *.avi; *.mkv; *.mp4";
            loadVideoDialog.ShowDialog();
            if (loadVideoDialog.FileName != "")
            {
                this.txtVideoUrl.Text = loadVideoDialog.FileName;
            }
        }

        void DrawText(Bitmap bmp)
        {
            RectangleF rectf = new RectangleF(0, 0, 0, 0);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(DateTime.Now.ToString(), new Font("Tahoma", 30), Brushes.GreenYellow, rectf);
            g.Flush();
        }

        Queue<Bitmap> _queue = new Queue<Bitmap>();
        Object lockObj1 = new Object();

        Queue<Bitmap> _queue2 = new Queue<Bitmap>();
        Object lockObj2 = new Object();

        #endregion

        #region Events

        private void btGhichu_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                int qCount = _queue.Count;
                lock (lockObj2)
                {
                    for (int i = 0; i < qCount; i++)
                    {
                        if (_queue.Count == 0)
                            break;
                        Thread.Sleep(100);
                        Bitmap bmp = _queue.Dequeue();
                        _queue2.Enqueue(bmp);
                        this.Invoke(new Action(() =>
                        {
                            pBImage.Image = _queue2.Dequeue();
                            progressBar2.Value = (int)(i / TotalFrame * 100);
                        }));
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private void btSelectfile_Click(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void btExtract_Click(object sender, EventArgs e)
        {
            if (txtVideoUrl.Text != "")
            {
                new Thread(() =>
                {
                    axWindowsMediaPlayer1.URL = txtVideoUrl.Text;
                    // Tạo một đối tượng VideoFileReader
                    VideoFileReader reader = new VideoFileReader();
                    // Mở file Video
                    reader.Open(txtVideoUrl.Text);
                    // Đọc khung hình
                    TotalFrame = reader.FrameCount;
                    lock (lockObj1)
                    {
                        for (int i = 0; i < TotalFrame; i++)
                        {
                            try
                            {
                                Bitmap videoFrame = reader.ReadVideoFrame();
                                Bitmap bmp = videoFrame;
                                DrawText(bmp);
                                _queue.Enqueue(bmp);
                                progressBar1.Value = (int)(i / TotalFrame * 100);
                                Thread.Sleep(30);
                                //videoFrame.Dispose();
                            }
                            catch (Exception err)
                            {
                                MessageBox.Show(err.Message);
                            }
                        }
                    }
                    reader.Close();
                })
                { IsBackground = true }.Start();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tệp video");
            }
        }
        #endregion
    }
}
