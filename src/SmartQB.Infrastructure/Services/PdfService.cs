using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using SmartQB.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SmartQB.Infrastructure.Services;

public class PdfService : IPdfService
{
    public int GetPageCount(string filePath)
    {
        using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions());
        return docReader.GetPageCount();
    }

    public Task<byte[]> RenderPageAsync(string filePath, int pageIndex)
        => Task.FromResult(RenderPage(filePath, pageIndex));

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
