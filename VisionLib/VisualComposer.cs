using System;
using System.Threading.Tasks;
using OpenCvSharp;
using Husty;

namespace VisionLib
{
    public class VisualComposer : IDisposable
    {

        // ------ Fields ------ //

        private readonly IDisposable _connector;
        private readonly ImageStream _cap;
        private readonly VisualErrorEstimator _estimator;
        private readonly VideoRecorder _recorder;
        private readonly Channel<Errors> _channel;
        private readonly Size _viewFrameSize = new(640, 480);
        private readonly Size _viewRadarSize = new(512, 512);


        // ------ Constructors ------ //

        public VisualComposer(bool visualize = false, bool rec = false)
        {
            _channel = new();
            _estimator = new();
            _cap = new(1);
            _recorder = rec ? new(_cap.FrameSize) : null;
            _connector = _cap.GetStream()
                .Subscribe(async frame =>
                {
                    _recorder?.Write(frame);
                    var errors = _estimator.Run(frame);
                    await _channel.WriteAsync(errors);

                    // ↓可視化用です。やや重くなるので走行時は非推奨
                    if (visualize)
                    {
                        using var radar = _estimator.GetGroundCoordinateView();
                        Cv2.Resize(frame, frame, _viewFrameSize);
                        Cv2.Resize(radar, radar, _viewRadarSize);
                        Cv2.ImShow("FRAME", frame);
                       /* Cv2.ImWrite("C:/Users/Ryo　Fujii/Desktop/a.jpg", frame);*/ //つけたし//
                        Cv2.ImShow("RADAR", radar);
                        Cv2.WaitKey(1);
                    }
                });
        }


        // ------ Methods ------ //

        public async Task<Errors> GetCurrentErrors()
        {
            var (suc, errors) = await _channel.ReadAsync();
            if (suc)
                return errors;
            else
                return new(0, 0);
        }


        public void Dispose()
        {
            _connector?.Dispose();
            _cap?.Dispose();
            _recorder?.Dispose();
            _channel?.Dispose();
        }

    }
}
