using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using OpenCvSharp;

namespace VisionLib
{
    internal class MockImageStream : IDisposable
    {

        public MockImageStream(int dummy = 0)
        {

        }

        public bool Read(ref Mat frame)
        {
            return true;
        }

        public IObservable<Mat> GetStream()
        {
            var frame = new Mat(960, 1280, MatType.CV_8UC3, 0);
                return Observable.Repeat(0, ThreadPoolScheduler.Instance)
                    .Select(_ =>
                    {
                        Thread.Sleep(100);
                        return frame;
                    }).Publish().RefCount();
        }

        public void Dispose()
        {

        }
    }
}
