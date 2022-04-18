using OpenCvSharp;

using Range = OpenCvSharp.Range;

Console.WriteLine("Running...");

Mat imgBase = Cv2.ImRead("./Assets/football.jpg");
Mat imgTemplate = Cv2.ImRead("./Assets/ball.jpg");

IEnumerable<TemplateMatchModes> matchModes = Enum.GetValues(typeof(TemplateMatchModes)).Cast<TemplateMatchModes>();

Parallel.ForEach(matchModes, mode =>
{
    Mat imgOutput = new();
    imgBase.CopyTo(imgOutput);

    Mat matchResult = new();
    Cv2.MatchTemplate(imgOutput, imgTemplate, matchResult, mode);

    Cv2.MinMaxLoc(matchResult, out _, out _, out Point minLoc, out Point maxLoc);

    Point location = (mode is TemplateMatchModes.SqDiff or TemplateMatchModes.SqDiffNormed) ? minLoc : maxLoc;

    Rect rect = new(location, new Size(imgTemplate.Width, imgTemplate.Height));

    Mat imgMatched = imgOutput[rect];

    double similarity = imgTemplate.ComputeSimilarity(imgMatched, out Mat difference, 9);

    Cv2.Rectangle(imgOutput, rect, Scalar.Red, 10);

    //Save result in Output folder
    Cv2.ImWrite($"output-{mode}-{similarity:#.##}.jpg", imgOutput);
    Cv2.ImWrite($"output-{mode}-matched.jpg", imgMatched);
    Cv2.ImWrite($"output-{mode}-DIFF.jpg", difference);
});

internal static class MatExtensions
{
    /// <summary>
    /// Makes the larger image the same size as the smaller image
    /// </summary>
    /// <param name="image1"></param>
    /// <param name="image2"></param>
    public static void NormalizeSize(this Mat image1, Mat image2)
    {
        if (image1.Width > image2.Width || image1.Height > image2.Height)
            image1.Trim(image2);

        if (image1.Width < image2.Width || image1.Height < image2.Height)
            image2.Trim(image1);
    }

    /// <summary>
    /// Makes the image grayscale
    /// </summary>
    /// <param name="image"></param>
    public static void ToBGR2GRAY(this Mat image)
    {
        if (image.Channels() > 1)
            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
    }

    /// <summary>
    /// Calculates the similarity based on the number of non-zeros in the difference
    /// </summary>
    /// <param name="imgTemplate"></param>
    /// <param name="imgMatched"></param>
    /// <param name="matDifference"></param>
    /// <param name="threshold"></param>
    /// <returns>Percentage of similarity</returns>
    public static double ComputeSimilarity(this Mat imgTemplate, Mat imgMatched, out Mat matDifference, double threshold = 9)
    {
        matDifference = new();
        Mat matNonZeros = new();

        try
        {
            if (imgTemplate.Width == 0 || imgMatched.Width == 0)
                return 0;

            imgTemplate.NormalizeSize(imgMatched);
            imgTemplate.ToBGR2GRAY();
            imgMatched.ToBGR2GRAY();

            Cv2.Absdiff(imgTemplate, imgMatched, matDifference);
            Cv2.Threshold(matDifference, matNonZeros, threshold, 255, ThresholdTypes.Binary);

            int nonZeros = matNonZeros.CountNonZero();

            double similarity = 100.0 * (1.0 - nonZeros / (double)(matDifference.Rows * matDifference.Cols));

            return similarity;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Compute Similarity Error: {e.Message}");
        }

        return 0;
    }

    private static void Trim(this Mat image, Mat smallerImage)
    {
        Range cols = new(0, smallerImage.Width);
        Range rows = new(0, smallerImage.Height);

        using Mat newImage = new(image, rows, cols);
        newImage.CopyTo(image);
    }
}