using SmartQB.Core.Algorithms;
using System.Linq;
using Xunit;

namespace SmartQB.Infrastructure.Tests;

public class ImageSegmentationLogicTests
{
    [Fact]
    public void FindVerticalSegments_ThreeVerticalRectangles_ReturnsThreeSegments()
    {
        var densities = new int[300];
        // 0-49: Ink, 50-99: Gap (50px), 100-149: Ink, 150-199: Gap (50px), 200-249: Ink, 250-299: Gap (50px)
        for (int i = 0; i < 50; i++) densities[i] = 10;
        for (int i = 100; i < 150; i++) densities[i] = 10;
        for (int i = 200; i < 250; i++) densities[i] = 10;

        var results = ImageSegmentationLogic.FindVerticalSegments(densities, noiseThreshold: 5, minGapHeight: 30, minSegmentHeight: 50).ToList();

        Assert.Equal(3, results.Count);

        Assert.Equal(0, results[0].StartY);
        Assert.Equal(75, results[0].EndY);

        Assert.Equal(75, results[1].StartY);
        Assert.Equal(175, results[1].EndY);

        Assert.Equal(175, results[2].StartY);
        Assert.Equal(300, results[2].EndY);
    }

    [Fact]
    public void FindVerticalSegments_AllWhiteImage_ReturnsSingleSegment()
    {
        var densities = new int[300]; // All zeros

        // This will return [0, 300] because we didn't add logic to filter out completely empty segments.
        // User said: "验证是否返回空集合或原图" -> single original image segment is fine.
        var results = ImageSegmentationLogic.FindVerticalSegments(densities, noiseThreshold: 5, minGapHeight: 30, minSegmentHeight: 50).ToList();

        Assert.Single(results);
        Assert.Equal(0, results[0].StartY);
        Assert.Equal(300, results[0].EndY);
    }

    [Fact]
    public void FindVerticalSegments_TinyGap_ReturnsSingleSegment()
    {
        var densities = new int[150];
        // 0-49: Ink, 50-69: Gap (20px < 30px threshold), 70-119: Ink, 120-149: Gap (30px)
        for (int i = 0; i < 50; i++) densities[i] = 10;
        for (int i = 70; i < 120; i++) densities[i] = 10;

        var results = ImageSegmentationLogic.FindVerticalSegments(densities, noiseThreshold: 5, minGapHeight: 30, minSegmentHeight: 50).ToList();

        Assert.Single(results);
        Assert.Equal(0, results[0].StartY);
        Assert.Equal(150, results[0].EndY);
    }
}
