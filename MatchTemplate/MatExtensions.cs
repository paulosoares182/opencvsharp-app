using OpenCvSharp;

using Range = OpenCvSharp.Range;

namespace OpenCvSharpApp
{
    internal static class MatExtensions
    {
        /// <summary>
        /// Makes the larger image the same size as the smaller image
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        public static void Trim(this Mat image1, Mat image2)
        {
            if (image1.Width > image2.Width || image1.Height > image2.Height)
                image1.Normalize(image2);

            if(image1.Width < image2.Width || image1.Height < image2.Height)
                image2.Trim(image1);
        }

        private static void Normalize(this Mat image, Mat smallerImage)
        {
            Range cols = new(0, smallerImage.Width);
            Range rows = new(0, smallerImage.Height);

            using Mat newImage = new(image, rows, cols);
            newImage.CopyTo(image);
        }
    }
}