using System;
using System.Linq;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;

namespace VisionLib
{

    internal class ImageStream : IDisposable
    {

        // ------ Fields ------ //

        private readonly VideoCapture _cap;
        private readonly bool _isFileSource;
        private readonly object _lockObj = new();


        // ------ Properties ------ //

        public double Fps { get; }

        public int FrameCount { get; }

        public Size FrameSize { get; }


        // ------ Constructors ------ //

        public ImageStream(int deviceId)
        {
            _cap = new(deviceId);
            _cap.Set(VideoCaptureProperties.FrameWidth, 1280);
            _cap.Set(VideoCaptureProperties.FrameHeight, 960);
            _cap.Set(VideoCaptureProperties.Fps, 30);
            Fps = _cap.Fps;
            FrameCount = -1;
            _isFileSource = false;
        }

        public ImageStream(string sourceFile)
        {
            _cap = new(sourceFile);
            Fps = _cap.Fps;
            FrameCount = _cap.FrameCount;
            _isFileSource = true;
        }


        // ------ Methods ------ //

        public bool Read(ref Mat frame)
        {
            return _cap.Read(frame);
        }

        public IObservable<Mat> GetStream()
        {
            var frame = new Mat();
            if (_isFileSource)
            {
                return Observable.Range(0, FrameCount, ThreadPoolScheduler.Instance)
                    .Select(_ =>
                    {
                        lock (_lockObj)
                        {
                            try
                            {
                                _cap?.Read(frame);
                                Thread.Sleep(1000 / (int)Fps);
                            }
                            catch { }
                        }
                        return frame;
                    })
                    .Where(f => !f.Empty())
                    .Publish().RefCount();
            }
            else
            {
                return Observable.Repeat(0, ThreadPoolScheduler.Instance)
                    .Select(_ =>
                    {
                        lock (_lockObj)
                        {
                            try 
                            { 
                                _cap?.Read(frame);
                                Cv2.Flip(frame, frame, FlipMode.XY);
                            }
                            catch { }
                        }
                        return frame;
                    })
                    .Where(f => !f.Empty())
                    .Publish().RefCount();
            }
        }

        public void Dispose()
        {
            _cap?.Dispose();
        }

    }
}
