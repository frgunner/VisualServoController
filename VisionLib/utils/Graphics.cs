using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace VisionLib
{
    internal static class Graphics
    {

        private static Scalar _red = new(0, 0, 200);

        internal static void DrawBoxes(Mat input, IEnumerable<Rect> boxes)
        {
            boxes.ToList().ForEach(b => Cv2.Rectangle(input, b, _red, 2));
        }

        internal static void DrawPoints(Mat input, IEnumerable<Point> points)
        {
            points.ToList().ForEach(p => Cv2.Circle(input, p, 3, _red, 3));
        }

    }
}
