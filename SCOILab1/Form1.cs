using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCOILab1
{
    public partial class Form1 : Form
    {
        private List<Image> loadedImages = new List<Image>();
        
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
                    Image image = Image.FromFile(fileName);
                    loadedImages.Add(image);
                }

                MessageBox.Show($"Successfully loaded {openFileDialog.FileNames.Length} images.");

                DisplayImages();
            }

        }

        private void DisplayImages()
        {
            flowLayoutPanelImages.Controls.Clear();

            foreach (Image image in loadedImages)
            {
                PictureBox pictureBox1 = new PictureBox();
                pictureBox1.Visible = true;
                pictureBox1.Image = image;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Width = 200;
                pictureBox1.Height = 150;
                
                flowLayoutPanelImages.Controls.Add(pictureBox1);

                ComboBox comboBox1 = new ComboBox();

                comboBox1.Visible = true;
                comboBox1.Items.AddRange(new object[] { "None", "Add", "Subtract", "Multiply" });
                comboBox1.SelectedIndex = 0; // Устанавливаем по умолчанию "None"
                comboBox1.Location = new Point(pictureBox1.Right + 10, pictureBox1.Top);
                flowLayoutPanelImages.Controls.Add(comboBox1);
              

                TrackBar transparencyTrackBar = new TrackBar();

                transparencyTrackBar.Minimum = 0;
                transparencyTrackBar.Maximum = 255;
                transparencyTrackBar.Value = 255; // Устанавливаем максимальную прозрачность по умолчанию
                transparencyTrackBar.Location = new Point(pictureBox1.Right + 10, pictureBox1.Bottom + 10);
                flowLayoutPanelImages.Controls.Add(transparencyTrackBar);

                comboBox1.SelectedIndexChanged += (sender, e) =>
                {
                    if (comboBox1.SelectedIndex.Equals(1))
                    {
                        MessageBox.Show("Add");
                    }
                    
                    // Обработка изменения выбранного метода наложения
                    // Обновление изображения с учетом выбранного метода
                };

                transparencyTrackBar.Scroll += (sender, e) =>
                {
                    // Обработка изменения прозрачности изображения
                    // Изменение прозрачности изображения в соответствии со значением трекбара
                };
                

            }
        }


        private async void UpdateResultImageR()
        {
            //pictureBoxResult.Image = pictureBox1.Image;
            while (true)
            {
                if (loadedImages.Count > 0)
                {
                    pictureBoxResult.Image = loadedImages[0];
                    pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
                }
                 
                //pictureBoxResult.Controls.Add(pictureBox1);

                // Обработка изображения в реальном времени
                // Например, применение фильтров или эффектов к изображению
                // Обновление результирующей картинки

                // Пример:
                //pictureBoxResult.Image = await ApplyImageProcessingAsync(originalImage);

                // Чтобы уменьшить нагрузку на процессор, рекомендуется добавить задержку
                await Task.Delay(100); // задержка в 100 миллисекунд для обновления изображения каждые 100 мс
            }
        }

        //private async Task<Image> ApplyImageProcessingAsync(Image inputImage)
        //{
        //    // Асинхронный метод для обработки изображения
        //    // Например, применение фильтров или эффектов к изображению
        //    // Возвращение обработанного изображения

        //    // Ваш код обработки изображения здесь

        //    return processedImage;
        //}
    }
}
