using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace VargPlot;

public sealed class SignalValuesRecorder : IDisposable
{
    private readonly object _gate = new();
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>
    /// Creates a new CSV recorder. If <paramref name="filePath"/> is a directory,
    /// a timestamped file is created inside it. If it's a file path, it’s used as-is.
    /// </summary>
    public SignalValuesRecorder(string filePath)
    {
        // Auto-flush is false for performance; we Flush() on Dispose.
        _writer = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan))
        {
            AutoFlush = false,
            NewLine = "\n"
        };
    }

    /// <summary>
    /// Records one chunk (same signature as your AppendChunk).
    /// Writes: time,value0,value1,... as CSV with invariant culture.
    /// Thread-safe.
    /// </summary>
    public void RecordChunk(Chunk chunk)
    {
        if (chunk.Values is null) return;
        if (_disposed) return;

        // Build CSV line
        var sb = new StringBuilder(32 + chunk.Values.Count * 12);
        sb.Append(chunk.Time.ToString("R", CultureInfo.InvariantCulture));

        for (int i = 0; i < chunk.Values.Count; i++)
        {
            sb.Append(',');
            sb.Append(chunk.Values[i].ToString("R", CultureInfo.InvariantCulture));
        }

        lock (_gate)
        {
            if (_disposed) return;
            _writer.WriteLine(sb.ToString());
        }
    }

    /// <summary>Flush pending data to disk.</summary>
    public void Flush()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _writer.Flush();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _writer.Flush();
            _writer.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}