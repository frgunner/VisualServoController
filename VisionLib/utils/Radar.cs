using OpenCvSharp;
using static OpenCvSharp.Cv2;

namespace VisionLib
{
    internal class Radar
    {

        // ------ Fields ------ //

        private readonly Size _size;
        private readonly Scalar _red = new(0, 0, 255);
        private readonly Scalar _green = new(0, 180, 0);
        private readonly Scalar _blue = new(255, 0, 0);
        private readonly Mat _radarCanvas;
        private readonly int _maxWidth;
        private readonly int _maxDistance;


        // ------ Constructors ------ //

        internal Radar(int maxWidth, int maxDistance, int? focusWidth = null)
        {
            _maxWidth = maxWidth;
            _maxDistance = maxDistance;
            _radarCanvas = new Mat(maxDistance, maxWidth * 2, MatType.CV_8UC3, 0);
            var w2 = _radarCanvas.Width / 2;
            var h = _radarCanvas.Height;
            Circle(_radarCanvas, new Point(w2, h), 1000, _blue, 14);
            Circle(_radarCanvas, new Point(w2, h), 2000, _blue, 14);
            Circle(_radarCanvas, new Point(w2, h), 3000, _blue, 14);
            Circle(_radarCanvas, new Point(w2, h), 4000, _blue, 14);
            Circle(_radarCanvas, new Point(w2, h), 5000, _blue, 14);
            Circle(_radarCanvas, new Point(w2, h), 6000, _blue, 14);
            if (focusWidth is not null)
            {
                var left = maxWidth - (int)focusWidth;
                var right = maxWidth + (int)focusWidth;
                Line(_radarCanvas, new Point(left, 0), new Point(left, h), _green, 10);
                Line(_radarCanvas, new Point(right, 0), new Point(right, h), _green, 10);
            }
            _size = new(maxWidth * 2 / 10, maxDistance / 10);
        }


        // ------ Methods ------ //

        internal Mat GetRadar(Point2f[] points, Point2f? p1 = null, Point2f? p2 = null)
        {
            var radar = _radarCanvas.Clone();
            foreach (var p in points)
                Circle(radar, GetRadarCoordinate(p), 100, _red, FILLED);
            if (p1 is Point2f point1 && p2 is Point2f point2)
            {
                var radarP1 = GetRadarCoordinate(point1);
                var radarP2 = GetRadarCoordinate(point2);
                Line(radar, radarP1, radarP2, new(0, 0, 180), 30);
            }
            Resize(radar, radar, _size);
            return radar;
        }

        private Point GetRadarCoordinate(Point2f p) =>
            new Point(p.X + _maxWidth, _maxDistance - p.Y);

    }
}
