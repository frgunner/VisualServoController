using System;
using System.Linq;
using System.Collections.Generic;
using OpenCvSharp;
using Husty.OpenCvSharp;

namespace VisionLib
{

    public record Errors(double LateralError, double HeadingError);

    internal class VisualErrorEstimator
    {

        // ------ Fields ------ //

        private readonly Size _size = new(640, 480);
        private readonly IntrinsicCameraParameters _paramIn;
        private readonly ExtrinsicCameraParameters _paramEx;
        private readonly PerspectiveTransformer _transformer;
        private readonly YoloDetector _detector;
        private readonly Radar _radar;
        private readonly HoughSingleLine _hough;
        private Point2f[] _points;
        private Point2f? _p1;
        private Point2f? _p2;

        // 見る範囲を制限したければここで
        // 射影変換後の座標系XY(mm)です
        private const int _maxWidth = 3000;
        private const int _maxDistance = 9000;
        private const int _focusWidth = 2000;


        // ------ Constructors ------ //

        internal VisualErrorEstimator()
        {
            // カメラキャリブレーションファイル置き場
            _paramIn = IntrinsicCameraParameters.Load("..\\..\\..\\..\\calib\\intrinsic.json");
            _paramEx = ExtrinsicCameraParameters.Load("..\\..\\..\\..\\calib\\extrinsic.json");
            _transformer = new(_paramIn.CameraMatrix, _paramEx);

            // YOLOのモデル置き場
            var cfg = "..\\..\\..\\..\\model\\_.cfg";
            var weights = "..\\..\\..\\..\\model\\_.weights";
            var names = "..\\..\\..\\..\\model\\_.names";
            _detector = new(cfg, weights, names, _size, 0.05f);

            _radar = new(_maxWidth, _maxDistance, _focusWidth);
            //ハフ変換設定
             _hough = new HoughSingleLine(
                 -20 * Math.PI / 180, 20 * Math.PI / 180, -1000, 1000, 
                 -1000, 1000, 0, 9000, 
                 0.25 * Math.PI / 180, 50, 50, 50
             );
        }
        

        // ------ Public Methods ------ //

        internal Errors Run(Mat frame)
        {
            // このへんは見なくていいです
            Cv2.Resize(frame, frame, _size);
            using var copy = frame.Clone();
            Cv2.Undistort(copy, frame, _paramIn.CameraMatrix, _paramIn.DistortionCoeffs);
            _points = GetPoints(frame);
            return DoEstimateErrors(_points);
        }

        internal Mat GetGroundCoordinateView()
        {
            return _radar.GetRadar(_points, _p1, _p2);
        }

        // ------ Private Methods ------ //

        private Point2f[] GetPoints(Mat input)
        {
            var w = input.Width;
            var h = input.Height;
            return _detector.Run(input)
                .Select(r =>
                {
                    r.DrawCenterPoint(input, new(0, 0, 180), 3);
                    return r.Box.Scale(w, h).ToRect().GetCenter();
                })
                .Select(c => _transformer.ConvertToWorldCoordinate(new(c.X, c.Y)))
                .ToArray();
        }

        private Errors DoEstimateErrors(IEnumerable<Point2f> points)
        {
            _p1 = null;
            _p2 = null;
            if (points.Count() <= 1)
                return new Errors(double.NaN, double.NaN);
            // ここを埋めてください
           
            var line = _hough.Run(points.ToPointArray()).ToLine2D();

            var lateralerror = line.DistanceTo(new(0, 0));
            var houghTheta = Math.Atan2(line.Slope, 1);
            var headingerror = 0.0;
            if (houghTheta >= 0)
                headingerror = Math.PI / 2 -houghTheta;
            else
                headingerror = -Math.PI / 2 - houghTheta;

            if (headingerror > Math.PI * 15 / 180 || headingerror < -Math.PI * 15 / 180)
                return new Errors(double.NaN, double.NaN);

            var y1 = 0;
            var x1 = line.GetX(y1);
            var y2 = 10000;
            var x2 = line.GetX(y2);
            _p1 = new Point(x1, y1);
            _p2 = new Point(x2, y2);
            
            var errors = new Errors(lateralerror, headingerror);
            return errors;
        }

    }
}
