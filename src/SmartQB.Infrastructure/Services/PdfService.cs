using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using SmartQB.Core.Algorithms;
using SmartQB.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQB.Infrastructure.Services;

public class PdfService : IPdfService
{
    private const int GAP_THRESHOLD = 30; // Minimum white space (pixels) to be considered a split
    private const int NOISE_THRESHOLD = 5; // Allow some noise (5 pixels)
    private const int MIN_QUESTION_HEIGHT = 50; // Ignore tiny slices (e.g. noise)

    public int GetPageCount(string filePath)
    {
        using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions());
        return docReader.GetPageCount();
    }

    public Task<byte[]> RenderPageAsync(string filePath, int pageIndex)
        => Task.FromResult(RenderPage(filePath, pageIndex));

    public Task<List<byte[]>> ExtractQuestionImagesAsync(string filePath, int pageIndex)
    {
        return Task.Run(() =>
        {
            var results = new List<byte[]>();

            // 1. Render the full page to SkiaSharp Image
            using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions());
            using var pageReader = docReader.GetPageReader(pageIndex);

            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();
            var rawBytes = pageReader.GetImage(); // BGRA32

            using var data = SKData.CreateCopy(rawBytes);
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var originalImage = SKImage.FromPixels(info, data);

            if (originalImage == null)
                return new List<byte[]>(); // Handle error

            using var bitmap = SKBitmap.FromImage(originalImage);

            // 2. Calculate Horizontal Projection Profile
            var rowInkDensity = new int[height];

            // Convert to grayscale for analysis to simplify thresholding
            for (int y = 0; y < height; y++)
            {
                int inkCount = 0;
                for (int x = 0; x < width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    // Simple luminance calculation or just check if not white
                    if (color.Red < 240 || color.Green < 240 || color.Blue < 240)
                    {
                        inkCount++;
                    }
                }
                rowInkDensity[y] = inkCount;
            }

            // 3. Call the pure logic algorithm
            var segments = ImageSegmentationLogic.FindVerticalSegments(rowInkDensity, NOISE_THRESHOLD, GAP_THRESHOLD, MIN_QUESTION_HEIGHT).ToList();

            // 4. Crop and Save Images
            foreach (var segment in segments)
            {
                int sliceHeight = segment.EndY - segment.StartY;

                // Create subset image
                using var subset = new SKBitmap(width, sliceHeight);

                using (var canvas = new SKCanvas(subset))
                {
                    // Draw white background first
                    canvas.Clear(SKColors.White);

                    var srcRect = new SKRect(0, segment.StartY, width, segment.EndY);
                    var destRect = new SKRect(0, 0, width, sliceHeight);

                    // Draw original image onto this canvas
                    canvas.DrawBitmap(bitmap, srcRect, destRect);
                }

                using var imageSubset = SKImage.FromBitmap(subset);
                using var dataSubset = imageSubset.Encode(SKEncodedImageFormat.Png, 100);
                results.Add(dataSubset.ToArray());
            }

            return results;
        });
    }

    private byte[] RenderPage(string filePath, int pageIndex)
    {
        using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions());
        using var pageReader = docReader.GetPageReader(pageIndex);

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();
        var rawBytes = pageReader.GetImage(); // BGRA32

        using var data = SKData.CreateCopy(rawBytes);

        // Docnet returns BGRA
        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        using var image = SKImage.FromPixels(info, data);
        using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);

        return encodedData.ToArray();
    }
}
