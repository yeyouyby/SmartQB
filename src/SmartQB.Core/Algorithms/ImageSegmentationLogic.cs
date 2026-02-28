using System;
using System.Collections.Generic;

namespace SmartQB.Core.Algorithms;

public static class ImageSegmentationLogic
{
    // Fix: cannot use ReadOnlySpan in an iterator method (yield return)
    // We can change the return type to List and return at the end
    public static IEnumerable<(int StartY, int EndY)> FindVerticalSegments(
        ReadOnlySpan<int> rowDensities,
        int noiseThreshold = 5,
        int minGapHeight = 30,
        int minSegmentHeight = 50)
    {
        var height = rowDensities.Length;
        var results = new List<(int StartY, int EndY)>();
        if (height == 0) return results;

        var cutPoints = new List<int> { 0 }; // Always start at the top

        bool inGap = true; // Assume we start in a top margin gap
        int currentGapSize = 0;

        for (int y = 0; y < height; y++)
        {
            // A row is considered 'empty' if the ink count is below the noise threshold
            bool isRowEmpty = rowDensities[y] < noiseThreshold;

            if (isRowEmpty)
            {
                if (!inGap)
                {
                    // Just entered a gap
                    inGap = true;
                    currentGapSize = 1;
                }
                else
                {
                    // Continuing in a gap
                    currentGapSize++;
                }
            }
            else
            {
                if (inGap)
                {
                    // Just exited a gap. Check if the gap was big enough to split.
                    if (currentGapSize >= minGapHeight)
                    {
                        // The optimal split point is the middle of the gap
                        int splitY = y - (currentGapSize / 2);
                        cutPoints.Add(splitY);
                    }
                    inGap = false;
                    currentGapSize = 0;
                }
            }
        }

        // Always add the bottom edge
        cutPoints.Add(height);

        // Process the raw cut points into segments, filtering out small noise segments
        for (int i = 0; i < cutPoints.Count - 1; i++)
        {
            int startY = cutPoints[i];
            int endY = cutPoints[i + 1];
            int segmentHeight = endY - startY;

            if (segmentHeight >= minSegmentHeight)
            {
                results.Add((startY, endY));
            }
        }

        return results;
    }
}
