using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace SCOILab1
{
    internal class ImageProcessing
    {
        static void MAain()
        {
            List<string> imagePaths = new List<string>
            {
                "image1.jpg",
                "image2.jpg",
                "image3.jpg"
                // Добавьте пути к вашим изображениям
            };

            List<Bitmap> images = new List<Bitmap>();

            foreach (string path in imagePaths)
            {
                Bitmap image = new Bitmap(path);
                images.Add(image);
            }

            int totalWidth = images[0].Width; // Допустим, начальная ширина равна ширине первого изображения
            int totalHeight = 0;

            foreach (Bitmap image in images)
            {
                totalHeight += image.Height;
                totalWidth = Math.Max(totalWidth, image.Width);
            }

            Bitmap resultImage = new Bitmap(totalWidth, totalHeight);
            using (Graphics graphics = Graphics.FromImage(resultImage))
            {
                int offsetY = 0;
                foreach (Bitmap image in images)
                {
                    graphics.DrawImage(image, 0, offsetY, image.Width, image.Height);
                    offsetY += image.Height;
                }
            }

            resultImage.Save("resultImage.jpg", ImageFormat.Jpeg);

            Console.WriteLine("Результирующее изображение успешно создано!");
        }
    }
}
