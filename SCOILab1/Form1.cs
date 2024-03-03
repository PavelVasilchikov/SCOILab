using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Net.Mime.MediaTypeNames;

namespace SCOILab1
{
    public partial class Form1 : Form
    {
        static private List<Bitmap> loadedBitImages = new List<Bitmap>();
        private List<string> imagePaths = new List<string>();
        private Bitmap outImage;

        static private int totalWidth; // Допустим, начальная ширина равна ширине первого изображения
        static private int totalHeight;

        string selectedBlendMethod = "None";




        public Form1()
        {
            InitializeComponent();
            InitializeImagePanel();
            UpdateResultImageR();
        }

        private void InitializeImagePanel()
        {
            flowLayoutPanelImages.AutoScroll = true;

            Controls.Add(flowLayoutPanelImages);
        }



        private void AddPic_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    //Image image = Image.FromFile(fileName);
                    //loadedImages.Add(image);
                    Bitmap imageBit = new Bitmap(fileName);
                    loadedBitImages.Add(imageBit);
                }

                MessageBox.Show($"Successfully loaded {openFileDialog.FileNames.Length} images.");

                DisplayImages();

                totalWidth = loadedBitImages[0].Width;
                totalHeight = loadedBitImages[0].Height;

                outImage = new Bitmap(totalWidth, totalHeight);
                outImage = loadedBitImages[loadedBitImages.Count-1];
                pictureBoxResult.Image = outImage;

            }

        }



        private void DisplayImages()
        {
            Bitmap previousImage = null;
            flowLayoutPanelImages.Controls.Clear();

            foreach (Bitmap image in loadedBitImages)
            {
                

                PictureBox pictureBox1 = new PictureBox();
                pictureBox1.Visible = true;
                pictureBox1.Image = image;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Width = 200;
                pictureBox1.Height = 150;

                ComboBox comboBox1 = new ComboBox();

                comboBox1.Visible = true;
                comboBox1.Items.AddRange(new object[] { "None", "Add", "Averge", "Multiply" });
                comboBox1.SelectedIndex = 0; // Устанавливаем по умолчанию "None"
                comboBox1.Location = new Point(pictureBox1.Right + 10, pictureBox1.Top);
                

                TrackBar transparencyTrackBar = new TrackBar();

                transparencyTrackBar.Minimum = 0;
                transparencyTrackBar.Maximum = 255;
                transparencyTrackBar.Value = 255; // Устанавливаем максимальную прозрачность по умолчанию
                transparencyTrackBar.Location = new Point(pictureBox1.Right + 10, pictureBox1.Bottom + 10);
                

                transparencyTrackBar.Scroll += (sender, e) =>
                {

                    float alpha = (float)transparencyTrackBar.Value / 255; // Преобразуем значение трекбара в диапазоне от 0 до 1 для прозрачности
                    Bitmap imageWithAlpha = ApplyTransparency(image, alpha); // Применяем прозрачность к изображению
                    pictureBox1.Image = imageWithAlpha; // Обновляем изображение с примененной прозрачностью
                };

                flowLayoutPanelImages.Controls.Add(pictureBox1);
                flowLayoutPanelImages.Controls.Add(comboBox1);
                flowLayoutPanelImages.Controls.Add(transparencyTrackBar);






                comboBox1.SelectedIndexChanged += (sender, e) =>
                {
                    selectedBlendMethod = comboBox1.SelectedItem.ToString();
                    if (previousImage != null)
                    {
                        ApplyBlendMethodAndUpdateImages(previousImage, (Bitmap)pictureBox1.Image, selectedBlendMethod, pictureBoxResult);
                    }
                    else
                    {
                        outImage = image;
                        pictureBox1.Image = outImage;
                    }
                };
                previousImage = image;

                

                
            }
        }

        private Dictionary<string, Func<Bitmap, Bitmap, Bitmap>> blendMethods = new Dictionary<string, Func<Bitmap, Bitmap, Bitmap>>()
        {
            { "None", (result, newImage) => result },
            { "Add", (result, newImage) => ApplyAddBlendMethod(result, newImage) },
            { "Averge", (result, newImage) =>  ApplyAveBlendMethod(result, newImage)},
            { "Multiply", (result, newImage) => ApplyMultiBlendMethod(result, newImage) }
        };

        private void ApplyBlendMethodAndUpdateImages(Bitmap prev, Bitmap current, string blendMethod, PictureBox pictureBox)
        {

            if (blendMethod == "None")
            {
                outImage = prev; // Просто отображаем текущее изображение без смешивания
                return;
            }

            if (outImage == null)
            {
                outImage = new Bitmap(loadedBitImages[loadedBitImages.Count-1].Width, loadedBitImages[loadedBitImages.Count - 1].Height);
            }

            // Используем промежуточное изображение, чтобы сохранить результат всех примененных эффектов смешивания
            using (Bitmap intermediateImage = new Bitmap(outImage))
            {
                outImage = blendMethods[blendMethod](intermediateImage, current);
            }

            pictureBox.Image = outImage;
        }

        static private Bitmap ApplyAddBlendMethod(Bitmap prev, Bitmap current)
        {

            // Создаем результирующее изображение такого же размера, как текущее изображение
            Bitmap result = new Bitmap(loadedBitImages[loadedBitImages.Count - 1].Width, loadedBitImages[loadedBitImages.Count - 1].Height);

            Bitmap lastImage = loadedBitImages[loadedBitImages.Count-1];


            for (int i = 0; i < lastImage.Height; ++i)
            {
                for (int j = 0; j < lastImage.Width; ++j)
                {
                    Color curPix;

                    if (i >= current.Height || j >= current.Width || i >= prev.Height || j >= prev.Width)
                    {
                        curPix = lastImage.GetPixel(j, i);
                        result.SetPixel(j, i, curPix);
                    }
                    else
                    {
                        // Получаем пиксели из последнего изображения
                        curPix = current.GetPixel(j, i);

                        // Получаем пиксели из предыдущего изображения
                        Color pix1 = prev.GetPixel(j, i);

                        // Вычисляем значения цветов для нового пикселя
                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;
                        int r2 = curPix.R;
                        int g2 = curPix.G;
                        int b2 = curPix.B;

                        int rFin = (int)Clamp(r1 + r2, 0, 255);
                        int gFin = (int)Clamp(g1 + g2, 0, 255);
                        int bFin = (int)Clamp(b1 + b2, 0, 255);

                        Color pixFin = Color.FromArgb(rFin, gFin, bFin);

                        // Записываем новый пиксель в результирующее изображение
                        result.SetPixel(j, i, pixFin);
                    }

                }
            }

            return result;
        }

        static private Bitmap ApplyAveBlendMethod(Bitmap prev, Bitmap current)
        {

            // Создаем результирующее изображение такого же размера, как текущее изображение
            Bitmap result = new Bitmap(loadedBitImages[loadedBitImages.Count - 1].Width, loadedBitImages[loadedBitImages.Count - 1].Height);

            Bitmap lastImage = loadedBitImages[loadedBitImages.Count - 1];


            for (int i = 0; i < lastImage.Height; ++i)
            {
                for (int j = 0; j < lastImage.Width; ++j)
                {
                    Color curPix;

                    if (i >= current.Height || j >= current.Width || i >= prev.Height || j >= prev.Width)
                    {
                        curPix = lastImage.GetPixel(j, i);
                        result.SetPixel(j, i, curPix);
                    }
                    else
                    {
                        // Получаем пиксели из последнего изображения
                        curPix = current.GetPixel(j, i);

                        // Получаем пиксели из предыдущего изображения
                        Color pix1 = prev.GetPixel(j, i);

                        // Вычисляем значения цветов для нового пикселя
                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;
                        int r2 = curPix.R;
                        int g2 = curPix.G;
                        int b2 = curPix.B;

                        int rFin = (int)Clamp((r1 + r2) / 2, 0, 255);
                        int gFin = (int)Clamp((g1 + g2) / 2, 0, 255);
                        int bFin = (int)Clamp((b1 + b2) / 2, 0, 255);

                        Color pixFin = Color.FromArgb(rFin, gFin, bFin);

                        // Записываем новый пиксель в результирующее изображение
                        result.SetPixel(j, i, pixFin);
                    }

                }
            }

            return result;
        }

        static private Bitmap ApplyMultiBlendMethod(Bitmap prev, Bitmap current)
        {

            // Создаем результирующее изображение такого же размера, как текущее изображение
            Bitmap result = new Bitmap(loadedBitImages[loadedBitImages.Count - 1].Width, loadedBitImages[loadedBitImages.Count - 1].Height);

            Bitmap lastImage = loadedBitImages[loadedBitImages.Count - 1];


            for (int i = 0; i < lastImage.Height; ++i)
            {
                for (int j = 0; j < lastImage.Width; ++j)
                {
                    Color curPix;

                    if (i >= current.Height || j >= current.Width || i >= prev.Height || j >= prev.Width)
                    {
                        curPix = lastImage.GetPixel(j, i);
                        result.SetPixel(j, i, curPix);
                    }
                    else
                    {
                        // Получаем пиксели из последнего изображения
                        curPix = current.GetPixel(j, i);

                        // Получаем пиксели из предыдущего изображения
                        Color pix1 = prev.GetPixel(j, i);

                        // Вычисляем значения цветов для нового пикселя
                        int r1 = pix1.R;
                        int g1 = pix1.G;
                        int b1 = pix1.B;
                        int r2 = curPix.R;
                        int g2 = curPix.G;
                        int b2 = curPix.B;

                        int rFin = (int)Clamp((r1 * r2)/256, 0, 255);
                        int gFin = (int)Clamp((g1 * g2)/256, 0, 255);
                        int bFin = (int)Clamp((b1 * b2)/256, 0, 255);

                        Color pixFin = Color.FromArgb(rFin, gFin, bFin);

                        // Записываем новый пиксель в результирующее изображение
                        result.SetPixel(j, i, pixFin);
                    }

                }
            }

            return result;
        }

        private async void UpdateResultImageR()
        {
            while (true)
            {
                if (loadedBitImages.Count > 0)
                {
                    pictureBoxResult.Image = outImage;
                    pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
                }
                await Task.Delay(100); // задержка в 100 миллисекунд для обновления изображения каждые 100 мс
            }
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private Bitmap ApplyTransparency(Bitmap image, float alpha)
        {
            if (image == null)
            {
                return null;
            }

            Bitmap transparentImage = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(transparentImage);

            ColorMatrix matrix = new ColorMatrix
            {
                Matrix33 = alpha // Устанавливаем значение прозрачности в матрице цвета
            };

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default);

            g.DrawImage(image,
                new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel, attributes);

            g.Dispose();

            return transparentImage;
        }
    }
}
