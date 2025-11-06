using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia;          // for Rect
using System.Collections.Concurrent;

namespace VargPlot;
public static class SessionFileLoader
{
    // Starts a background task that reads "time,v0,v1,..." and enqueues Chunks.
    // Call exactly once when you enter LoadFileMode.
    public static Task Start(string filePath, ConcurrentQueue<Chunk> queue, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            using var sr = new StreamReader(filePath);
            string? line;

            while (!sr.EndOfStream && !ct.IsCancellationRequested)
            {
                line = await sr.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line[0] == '#' || line.StartsWith("//", StringComparison.Ordinal)) continue;

                if (TryParse(line, out float t, out List<float> values))
                    queue.Enqueue(new Chunk { Time = t, Values = values });
            }
        }, ct);
    }

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
