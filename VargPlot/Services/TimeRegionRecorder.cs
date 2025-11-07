using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot.Services
{
    public sealed class TimeRegionRecorder : IDisposable
    {
        private readonly object _gate = new();
        private readonly StreamWriter _writer;
        private bool _disposed;

        public TimeRegionRecorder(string filePath)
        {

            _writer = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = false,
                NewLine = "\n"
            };

            _writer.WriteLine("start");
        }

        public void RecordTimeRect(TimeRect rect)
        {
            if (rect is null) return;

            float s = Math.Min(rect.startTime, rect.endTime);
            float e = Math.Max(rect.startTime, rect.endTime);

            int type = rect.fillColor.Color.G == 0 ? 1 : 0;

            var sb = new StringBuilder();
            sb.AppendLine($"  rect  start={s.ToString("R", CultureInfo.InvariantCulture)}  " +
                          $"end={e.ToString("R", CultureInfo.InvariantCulture)} " +
                          $"type={type.ToString("R", CultureInfo.InvariantCulture)}");

            int n = rect.markers?.Count ?? 0;
            sb.AppendLine($"    markers={n}");

            if (n > 0)
            {
                foreach (var m in rect.markers!)
                {
                    int flag = MarkerFlag(m); // Red->0, Green->1
                    sb.AppendLine($"      m t={m.time.ToString("R", CultureInfo.InvariantCulture)} flag={flag}");
                }
            }

            sb.AppendLine("  endrect");

            lock (_gate)
            {
                if (_disposed) return;
                _writer.Write(sb.ToString());
            }

            Flush();
        }

        public void Flush()
        {
            lock (_gate) { if (!_disposed) _writer.Flush(); }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                _writer.WriteLine("end");
                _writer.Flush();
                _writer.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private static int MarkerFlag(TimeMarker m)
        {
            // Map flagColor to 0/1; fallback 0
            if (m.flagColor is SolidColorBrush scb)
            {
                var c = scb.Color;
                if (c.R >= c.G && c.R >= c.B) return 0; // red-ish
                if (c.G > c.R && c.G >= c.B) return 1; // green-ish
            }
            return 0;
        }

        static bool SameSolid(SolidColorBrush? a, SolidColorBrush? b)
        {
            if (a is null || b is null) return a is null && b is null;
            // Color includes A (alpha). Compare opacity too if you use it separately.
            return a.Color == b.Color && Math.Abs(a.Opacity - b.Opacity) < 1e-9;
        }
    }
}