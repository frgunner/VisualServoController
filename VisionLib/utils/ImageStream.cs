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

        public ImageStream(int deviceId, int width = 1280, int height = 960)
        {
            _cap = new(deviceId);
            _cap.Set(VideoCaptureProperties.FrameWidth, width);
            _cap.Set(VideoCaptureProperties.FrameHeight, height);
            Fps = _cap.Fps;
            FrameCount = -1;
            FrameSize = new(width, height);
            _isFileSource = false;
        }

        public ImageStream(string sourceFile, int width = 1280, int height = 960)
        {
            _cap = new(sourceFile);
            _cap.Set(VideoCaptureProperties.FrameWidth, width);
            _cap.Set(VideoCaptureProperties.FrameHeight, height);
            Fps = _cap.Fps;
            FrameCount = _cap.FrameCount;
            FrameSize = new(width, height);
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
