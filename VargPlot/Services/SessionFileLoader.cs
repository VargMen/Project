using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia;          // for Rect
using System.Collections.Concurrent;

namespace VargPlot;
public static class SessionFileLoader
{
    /// <summary>
    /// Reads CSV "time,v0,v1,..." and appends directly to Plot as fast as possible.
    /// requestUiRefresh should marshal to UI thread (e.g., () => Dispatcher.UIThread.Post(InvalidateVisual)).
    /// </summary>
    public static Task Start(string filePath, Plot plot, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                          bufferSize: 1 << 20, options: FileOptions.SequentialScan);
            using var sr = new StreamReader(fs);

            string? line;

            while (!sr.EndOfStream && !ct.IsCancellationRequested)
            {
                line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line[0] == '#' || line.StartsWith("//", StringComparison.Ordinal)) continue;

                if (!TryParse(line, out float t, out List<float> values)) continue;

                // Append directly. Guard against concurrent reads in Render().
                lock (plot) // simplest: use same lock in your Render() when reading plot lists
                {
                    plot.AppendChunk(new Chunk(t, values));
                }
            }
        }, ct);
    }

    // simple parser (fast enough). Expect: time,v0,v1,...
    private static bool TryParse(string line, out float time, out List<float> values)
    {
        time = 0f;
        values = new List<float>();

        var parts = line.Split(',');
        if (parts.Length < 2) return false;

        if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out time))
            return false;

        for (int i = 1; i < parts.Length; i++)
        {
            if (!float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return false;
            values.Add(v);
        }
        return true;
    }
}
