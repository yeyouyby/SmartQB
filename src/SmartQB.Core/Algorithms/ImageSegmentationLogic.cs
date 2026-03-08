using System;
using System.Collections.Generic;

namespace SmartQB.Core.Algorithms;

/// <summary>
/// Provides logic for segmenting images based on row pixel densities.
/// </summary>
public static class ImageSegmentationLogic
{
    /// <summary>
    /// Identifies vertical segments within an image based on row ink densities.
    /// </summary>
    /// <param name="rowDensities">A span containing the ink density for each row of the image.</param>
    /// <param name="noiseThreshold">The threshold below which a row is considered empty noise.</param>
    /// <param name="minGapHeight">The minimum number of consecutive empty rows required to split segments.</param>
    /// <param name="minSegmentHeight">The minimum height of a segment to be included in the results.</param>
    /// <returns>A list of tuples containing the start and end Y-coordinates of each segment.</returns>
    public static List<(int StartY, int EndY)> FindVerticalSegments(
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
            bool isRowEmpty = rowDensities[y] < noiseThreshold;

            if (isRowEmpty && !inGap)
            {
                // Just entered a gap
                inGap = true;
                currentGapSize = 1;
            }
            else if (isRowEmpty)
            {
                // Continuing in a gap
                currentGapSize++;
            }
            else if (inGap)
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
