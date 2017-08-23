using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace homework2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap tomato=null;
        FileDialog tomatofile;
        private void button1_Click(object sender, EventArgs e)
        {
            tomatofile = new OpenFileDialog();

            if (tomatofile.ShowDialog() == DialogResult.OK)
            {
                tomato = new Bitmap(tomatofile.FileName);
                source.Image = tomato;
                
            }
        }
        Bitmap noise = null;
        Bitmap pepnoise = null;
        Random rand = new Random();

        private void paper_Click(object sender, EventArgs e) //載入圖片
        {
            if (source.Image == null)
            {
                ShowError("尚未載入圖片");
                return;
            }
            double pepperRate = Convert.ToDouble(this.textBox1.Text);
            pepnoise = new Bitmap(tomatofile.FileName);
            int colortemp;
            int number = Convert.ToInt32(pepperRate * (tomato.Width * tomato.Height))/100;
            for(int i=0;i<number;i++)
            {
                colortemp = rand.Next(2) * 255;
                pepnoise.SetPixel(rand.Next(tomato.Width),rand.Next(tomato.Height), Color.FromArgb(colortemp, colortemp, colortemp));
            }
            noiseimg.Image = pepnoise;
        }

        private void gauss_Click(object sender, EventArgs e)
        {
            if (source.Image == null)
            {
                ShowError("尚未載入圖片");
                return;
            }
            Rectangle clone = new Rectangle(0, 0, tomato.Width, tomato.Height);
            noise = tomato.Clone(clone, System.Drawing.Imaging.PixelFormat.DontCare);
            int[,,] arr = new int[tomato.Height, tomato.Width,3];
            int width = tomato.Width;
            int height = tomato.Height;
            int i, j;
            Color abc;
            for( i=0;i<width;i++)
            {
                for( j=0;j<height;j++)
                {
                    abc = tomato.GetPixel(i, j);
                    arr[j, i,0] = abc.R;
                    arr[j, i, 1] = abc.G;
                    arr[j, i, 2] = abc.B;

                }
            }
            double Std = Convert.ToDouble(textBox2.Text);

            int[,] randnumber = new int[height, width];
            for (i = 0; i < height; i++)
                for (j = 0; j < width; j++)
                    randnumber[i, j] = rand.Next(256);
            double[] sourcehist = new double[256];
            for (i = 0; i < height; i++)
                for (j = 0; j < width; j++)
                    sourcehist[randnumber[i,j]]++;

            for (i = 0; i < 256; i++)
                sourcehist[i] = sourcehist[i] / (height * width);

            double[] sourcecdf = new double[256];
            sourcecdf[0] = sourcehist[0];
            for (i = 1; i < 256; i++)
                sourcecdf[i] = sourcecdf[i - 1] + sourcehist[i];

            double[] targethist = new double[256];
            for (i = 0; i < 256; i++)
                targethist[i] = (1 / Math.Sqrt((Math.PI * Std * Std)) )* 
                    Math.Exp(-1 * (i - 127) * (i - 127) / (Std * Std));

            double total = 0;
            for (i = 0; i < 256; i++)
                total += targethist[i] ;
            for (i = 0; i < 256; i++)
                targethist[i] = targethist[i] / total;

            double[] targetcdf = new double[256];
            targetcdf[0] = targethist[0];
            for (i = 1; i < 256; i++)
                targetcdf[i] = targetcdf[i - 1] + targethist[i];

            int[] map = new int[256];
            int nearest;
            for (i=0;i<256;i++)
            {
                nearest= 0;
                for (j = 0; j < 256; j++)
                {
                    if (Math.Abs(sourcecdf[i] - targetcdf[j]) < Math.Abs(sourcecdf[i] - targetcdf[nearest]))
                        nearest = j;
                }
                map[i] = nearest;
            }
            total = 0;
            for(i=0;i<height;i++)
            {
                for(j=0;j<width;j++)
                {
                    randnumber[i, j] = map[randnumber[i,j]];
                    total += randnumber[i, j];
                }
            }
            int mean = (int)(total / (double)(height * width));
            for (i = 0; i < height; i++)
                for (j = 0; j < width; j++)
                    randnumber[i, j] = randnumber[i,j] - mean;

            int[,,] result = new int[height , width,3];
            
            for( i=0;i<height;i++)
            {
                for (j = 0; j < width; j++)
                {
                    if (arr[i, j,0] + randnumber[i, j] > 255)
                        result[i, j,0] = 255;
                    else if (arr[i, j,0] + randnumber[i, j] < 0)
                        result[i, j,0] = 0;
                    else
                        result[i, j,0] = arr[i, j,0] + randnumber[i, j];

                    if (arr[i, j,1] + randnumber[i, j] > 255)
                        result[i, j,1] = 255;
                    else if (arr[i, j,1] + randnumber[i, j] < 0)
                        result[i, j,1] = 0;
                    else
                        result[i, j,1] = arr[i, j,1] + randnumber[i, j];

                    if (arr[i, j,2] + randnumber[i, j] > 255)
                        result[i, j,2] = 255;
                    else if (arr[i, j,2] + randnumber[i, j] < 0)
                        result[i, j,2] = 0;
                    else
                        result[i, j,2] = arr[i, j,2] + randnumber[i, j];
                }
            }
            for(i=0;i<height;i++)
            {
                for(j=0;j<width;j++)
                {
                    noise.SetPixel(j, i, Color.FromArgb(result[i, j,0], result[i, j,1], result[i, j,2]));
                }
            }

            gaussimg.Image = noise;

        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if (source.Image == null  )
            {
                ShowError("尚未載入圖片");
                return;
            }

            if( comboBox1.SelectedItem == null)
            {
                ShowError("尚未選擇矩陣SIZE");
                return;
            }
            string a = comboBox1.SelectedItem.ToString(); //取得矩陣SIZE
            int b = int.Parse(Convert.ToString(a[0]));


            if (noise == null && pepnoise == null)
            {
                ShowError("請灑雜訊再點擊過濾按鈕");
                return;
            }

            if (pepnoise!=null) //做胡椒
            {

                
                pictureBox2.Image= mean(pepnoise, b); 

            }
            if (noise!=null) //做高斯
            {
                
               pictureBox3.Image = mean(noise, b);
            }
        }

        public static void ShowError(string MessageText)  //錯誤視窗
        {
            MessageBox.Show(MessageText, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        int[] total=new int [3];
        Color temp;
        Bitmap dirty;
        Bitmap mean(Bitmap img,int k)
        {
            Rectangle clone = new Rectangle(0, 0, img.Width, img.Height);
            dirty = img.Clone(clone, System.Drawing.Imaging.PixelFormat.DontCare);

            for (int i=(k/2);i<(img.Width-(k/2));i++)
            {
                for(int j=(k/2);j<(img.Height-(k/2));j++)
                {//每個Kxk做一次
                    total[0] = 0;
                    total[1] = 0;
                    total[2] = 0;

                    for (int p=i-k/2;p<=i+k/2;p++)
                    {
                        for(int q=j-k/2;q<=j+k/2;q++)
                        {

                            temp = img.GetPixel(p, q);
                            
                            total[0] += temp.R;
                            total[1] += temp.G;
                            total[2] += temp.B;
                        }
                    }
                    total[0] /= k * k;
                    total[1] /= k * k;
                    total[2] /= k * k;
                    dirty.SetPixel(i, j, Color.FromArgb(total[0], total[1], total[2]));
                }
            }
            return dirty;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            if (source.Image == null)
            {
                ShowError("尚未載入圖片");
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                ShowError("尚未選擇矩陣SIZE");
                return;
            }
            string a = comboBox2.SelectedItem.ToString(); //取得矩陣SIZE
            int b = int.Parse(Convert.ToString(a[0]));

            
            if(noise==null &&pepnoise==null)
            {
                ShowError("請灑雜訊再點擊過濾按鈕");
                return;
            }

            if (pepnoise != null) //做胡椒
            {
                
                pictureBox2.Image = mid(pepnoise, b);

            }
            if (noise != null) //做高斯
            {

                pictureBox3.Image = mid(noise, b);
            }
        }
        Bitmap clean;
        Bitmap mid (Bitmap img,int k)
        {
            
            Rectangle clone = new Rectangle(0, 0, img.Width, img.Height);
            clean = img.Clone(clone, System.Drawing.Imaging.PixelFormat.DontCare);
            int[] queue = new int[k*k];
            int count,grayv,min=0,tempq=0,ii=0,jj=0;
            for(int i=k/2;i<img.Width -k/2;i++)
            {
                for(int j=k/2;j<img.Height-k/2;j++)
                {
                    count = 0;
                    for(int x=i-k/2;x<=i+k/2;x++)
                    {
                        for(int y= j - k/2; y <= j + k/2; y++)
                        {
                            temp = img.GetPixel(x, y);
                            grayv = (temp.R + temp.G + temp.B) / 3;
                            queue[count] = grayv;
                                count++;
                        }
                    }
                    for(int x=0;x<=(k* k)/2;x++)
                    {
                        min = 999;
                        for(int y= 0;y<k* k;y++)
                        {
                            if(queue[y]<min)
                            {
                                min = queue[y];
                                tempq = y;
                            }
                        }

                        queue[tempq] = 999;
                    }

                    for (int x = i - k / 2; x <= i + k / 2; x++)
                    {
                        for (int y = j - k / 2; y <= j + k / 2; y++)
                        {
                            temp = img.GetPixel(x, y);
                            grayv = (temp.R + temp.G + temp.B) / 3;
                            if(grayv==min)
                            {
                                ii = x;
                                jj = y;
                            }
                        }
                    }
                    
                    //clean.SetPixel(i, j, Color.FromArgb(min, min, min));
                    clean.SetPixel(i, j,img.GetPixel(ii, jj));
                }
            }
            return clean;
        }
    }
}
