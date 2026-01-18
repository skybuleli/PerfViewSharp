using System;
using System.Collections.Generic;
using System.Linq;

namespace PerfViewSharp.Core;

public class TraceBlock
{
    public string Name { get; set; } = "";
    public double StartTimeMSec { get; set; }
    public double DurationMSec { get; set; }
    public int Depth { get; set; }
    public int ThreadId { get; set; }
    public string? SourceFile { get; set; }
    public int LineNumber { get; set; }
}

public class MethodStats
{
    public string Name { get; set; } = "";
    public double TotalDurationMSec { get; set; }
    public int CallCount { get; set; }
    public double Percentage { get; set; }
}

public class TraceMetadata
{
    public string ProcessName { get; set; } = "SampleProcess";
    public double TotalDurationMSec { get; set; }
    public List<TraceBlock> Blocks { get; set; } = new();

    public MethodStats GetStatsForMethod(string methodName)
    {
        var relevantBlocks = Blocks.Where(b => b.Name == methodName).ToList();
        double total = relevantBlocks.Sum(b => b.DurationMSec);
        return new MethodStats
        {
            Name = methodName,
            TotalDurationMSec = total,
            CallCount = relevantBlocks.Count,
            Percentage = (total / TotalDurationMSec) * 100.0
        };
    }
}

public class TraceEngine 
{
    public TraceMetadata GenerateMockData(int blockCount = 500)
    {
        var metadata = new TraceMetadata { TotalDurationMSec = 10000 };
        var random = new Random(42);
        for (int t = 0; t < 4; t++) 
        {
            GenerateThreadData(metadata, t, 0, 10000, 0, random);
        }
        return metadata;
    }

    private void GenerateThreadData(TraceMetadata meta, int threadId, double start, double end, int depth, Random rnd)
    {
        if (depth > 8 || (end - start) < 50) return;
        double current = start;
        while (current < end)
        {
            double duration = (end - current) * rnd.NextDouble() * 0.8;
            if (duration < 20) break;
            var block = new TraceBlock
            {
                Name = $"Method_{rnd.Next(50)}", // 减少随机数范围以增加重复率，方便统计
                StartTimeMSec = current,
                DurationMSec = duration,
                Depth = depth,
                ThreadId = threadId,
                SourceFile = "/Users/liliang/PerfViewSharp/PerfViewSharp.UI/Program.cs",
                LineNumber = rnd.Next(1, 30)
            };
            meta.Blocks.Add(block);
            GenerateThreadData(meta, threadId, current, current + duration, depth + 1, rnd);
            current += duration + 10;
        }
    }
}