using System;
using System.IO;
using OpenCvSharp;

namespace VisionLib
{

    public class VideoRecorder : IDisposable
    {

        private readonly string _name;
        private VideoWriter _wrt;

        public VideoRecorder(Size? size)
        {
            if (!Directory.Exists("log")) Directory.CreateDirectory("log");
            Directory.CreateDirectory("log");
            var t = DateTimeOffset.Now;
            _name = $"{t.Year}{t.Month:d2}{t.Day:d2}{t.Hour:d2}{t.Minute:d2}{t.Second}";
            if (size is not null)
                _wrt = new($"log\\{_name}.mp4", FourCC.MPG4, 10, (Size)size);
        }

        public void Write(Mat frame)
        {
            if (_wrt is not null && !_wrt.IsDisposed)
                _wrt?.Write(frame);
        }

        public void Dispose()
        {
            _wrt?.Release();
            _wrt?.Dispose();
            _wrt = null;
        }

    }
}
