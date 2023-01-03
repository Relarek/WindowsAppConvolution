using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsAppConvolution
{
    public partial class Form1 : Form
    {
        public static string fileName;

        //0 Gaussian smoothing
        static double[,] kM0 = new double[5, 5]
        {
            { (double)2/159, (double)4/159,  (double)5/159,  (double)4/159,  (double)2/159 },
            { (double)4/159, (double)9/159,  (double)12/159, (double)9/159,  (double)4/159 },
            { (double)5/159, (double)12/159, (double)15/159, (double)12/159, (double)5/159 },
            { (double)4/159, (double)9/159,  (double)12/159, (double)9/159,  (double)4/159 },
            { (double)2/159, (double)4/159,  (double)5/159,  (double)4/159,  (double)2/159 }
        };

        static double[,,] kM = new double[10, 3, 3]
        {
            //1 smoothing
            {
                {0.11,0.11,0.11},
                {0.11,0.11,0.11},
                {0.11,0.11,0.11}
            },
            //2 sharpening 1
            {
                {-0.11, -0.11, -0.11},
                {-0.11, 2,     -0.11},
                {-0.11, -0.11, -0.11}
            },
            //3 increase the brightness 1
            {
                {-0.1, 0.2, -0.1},
                {0.2,    3, 0.2},
                {-0.1, 0.2, -0.1}
            },
            //4 decrease the brightness
            {
                {-0.1, 0.1, -0.1},
                {0.1, 0.5,     0.1},
                {-0.1, 0.1, -0.1}
            },

            //5 edge detection X Sobel
            {
                {1, 0, -1},
                {2, 0, -2},
                {1, 0, -1} 
            },
            //6 edge detection Y Sobel
            { 
                {1, 2, 1},
                {0, 0, 0},
                {-1,-2, -1} 
            },            

            //7 edge detection X Prewitt
            {
                {-1, 0, 1},
                {-1, 0, 1},
                {-1, 0, 1}
            },
		    //8 edge detection Y Prewitt
            {
                {-1, -1, -1},
                {0, 0, 0},
                {1, 1, 1}
            },   
		    //9 sharpening 2
            {
                {0,     -0.25,  0 },
                {-0.25, 2,      -0.25 },
                {0,     -0.25,  0 }
            },
		    //10 increase the brightness 2
            {
                {0.5, 1, 0.5 },
                {1,   2,   1 },
                {0.5, 1, 0.5 }
            }
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // открытие файла
            openFileDialog1.Filter = "All files (*.*)|*.*"; //(.bmp)|*.bmp|
            openFileDialog1.Title = "Open an Image File";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            fileName = openFileDialog1.FileName;

            // загрузка файла в pictureBox1
            Bitmap bmp = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = bmp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(5);

            for (int i = 0; i < kM0.GetLength(0); i++)
            {
                for (int j = 0; j < kM0.GetLength(1); j++)
                {
                    dataGridView1.Columns[j].Width = 60;
                    dataGridView1.Rows[i].Cells[j].Value = Math.Round(kM0[i, j], 3);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int mode = listBox1.SelectedIndex;
            if (mode == -1) 
            { 
                return; 
            }

            if (mode == 0)
            {
                Apply(kM0);                            
            }
            else
            {
                double[,] km = new double[3, 3];

                for (int i = 0; i < kM.GetLength(1); i++)
                    for (int j = 0; j < kM.GetLength(2); j++)
                    {
                        //km[i, j] = kM[mode-1, i, j];
                        km[i, j] = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value);
                    }

                Apply(km);
            }            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) { return; }

            int mode = listBox1.SelectedIndex;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (mode == 0)
            {
                for (int i = 0; i < kM0.GetLength(0); i++)
                {
                    dataGridView1.Columns.Add("Column" + (i + 1).ToString(), (i + 1).ToString());
                    dataGridView1.Columns[i].Width = 60;
                }
                dataGridView1.Rows.Add(5);

                for (int i = 0; i < kM0.GetLength(0); i++)
                {
                    for (int j = 0; j < kM0.GetLength(1); j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = Math.Round(kM0[i, j], 3);
                    }
                }
            }
            else
            {
                for (int i = 0; i < kM.GetLength(1); i++)
                {
                    dataGridView1.Columns.Add("Column" + (i + 1).ToString(), (i + 1).ToString());
                    dataGridView1.Columns[i].Width = 60;
                }
                dataGridView1.Rows.Add(3);

                for (int i = 0; i < kM.GetLength(1); i++)
                {
                    for (int j = 0; j < kM.GetLength(2); j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = kM[mode-1, i, j];
                    }
                }
            }
        }

        public static byte[] MyLoadBMP(Bitmap input)
        {
            BitmapData curBitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = curBitmapData.Stride;
            byte[] data = new byte[stride * input.Height];
            Marshal.Copy(curBitmapData.Scan0, data, 0, data.Length);
            input.UnlockBits(curBitmapData);

            byte[] outdata = new byte[input.Width * 3 * input.Height];

            for (int i = 0; i < input.Height; i++)
            {
                for (int j = 0; j < input.Width; j++)
                {
                    outdata[j * 3 + 0 + i * 3 * input.Width] = data[i * stride + j * 3 + 0];
                    outdata[j * 3 + 1 + i * 3 * input.Width] = data[i * stride + j * 3 + 1];
                    outdata[j * 3 + 2 + i * 3 * input.Width] = data[i * stride + j * 3 + 2];
                }
            }

            return outdata;
        }

        public void Apply(double[,] kernel)
        {
            if (fileName == null) return;

            Bitmap input = new Bitmap(fileName);
            Bitmap output = new Bitmap(fileName);

            byte[] inputBytes = MyLoadBMP(input);
            byte[] outputBytes = new byte[inputBytes.Length];

            int width = input.Width;
            int height = input.Height;

            int kernelWidth = kernel.GetLength(0);
            int kernelHeight = kernel.GetLength(1);

            int cR_, cB_, cG_;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double rSum = 0, gSum = 0, bSum = 0, kSum = 0;

                    for (int i = 0; i < kernelWidth; i++)
                    {
                        for (int j = 0; j < kernelHeight; j++)
                        {
                            int pixelPosX = x + (i - (kernelWidth / 2));
                            int pixelPosY = y + (j - (kernelHeight / 2));
                            if ((pixelPosX < 0) || (pixelPosX >= width) || (pixelPosY < 0) || (pixelPosY >= height))
                                continue;

                            byte r = inputBytes[3 * (width * pixelPosY + pixelPosX) + 0];
                            byte g = inputBytes[3 * (width * pixelPosY + pixelPosX) + 1];
                            byte b = inputBytes[3 * (width * pixelPosY + pixelPosX) + 2];

                            double kernelVal = kernel[i, j];

                            rSum += r * kernelVal;
                            gSum += g * kernelVal;
                            bSum += b * kernelVal;

                            kSum += kernelVal;
                        }
                    }

                    //if (kSum <= 0) 
                    kSum = 1;

                    //Контролируем переполнения переменных
                    rSum /= kSum;
                    if (rSum < 0) rSum = 0;
                    if (rSum > 255) rSum = 255;// 127;

                    gSum /= kSum;
                    if (gSum < 0) gSum = 0;
                    if (gSum > 255) gSum = 255;// 127;

                    bSum /= kSum;
                    if (bSum < 0) bSum = 0;
                    if (bSum > 255) bSum = 255;// 127;

                    //Записываем значения в результирующее изображение
                    outputBytes[3 * (width * y + x) + 0] = (byte)rSum;
                    outputBytes[3 * (width * y + x) + 1] = (byte)gSum;
                    outputBytes[3 * (width * y + x) + 2] = (byte)bSum;

                    cR_ = outputBytes[3 * (width * y + x) + 0];
                    cG_ = outputBytes[3 * (width * y + x) + 1];
                    cB_ = outputBytes[3 * (width * y + x) + 2];

                    //output.SetPixel(x, y, System.Drawing.Color.FromArgb(cR_, cG_, cB_));

                    output.SetPixel(x, y, System.Drawing.Color.FromArgb(
                        outputBytes[3 * (width * y + x) + 0],
                        outputBytes[3 * (width * y + x) + 1],
                        outputBytes[3 * (width * y + x) + 2]));
                }
            }

            MySaveBMP(outputBytes, width, height);

            return;
        }

        private void MySaveBMP(byte[] buffer, int width, int height)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // add back dummy bytes between lines, make each line be a multiple of 4 bytes
            int skipByte = bmpData.Stride - width * 3;
            byte[] newBuff = new byte[buffer.Length + skipByte * height];
            for (int j = 0; j < height; j++)
            {
                Buffer.BlockCopy(buffer, j * width * 3, newBuff, j * (width * 3 + skipByte), width * 3);                
            }

            // fill in rgbValues
            Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
            b.UnlockBits(bmpData);
            pictureBox2.Image = b;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // диалог сохранения файла
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.FileName = fileName.Replace(".", "Out.");
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";

            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)    // если отмена сохранения
            {
                return;
            }

            // если имя файла не пусто, сохраняем выходное изображение из pictureBox2 в файл
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        this.pictureBox2.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        this.pictureBox2.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        this.pictureBox2.Image.Save(fs,
                          System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
                fs.Close();
            }
        }
    }
}
