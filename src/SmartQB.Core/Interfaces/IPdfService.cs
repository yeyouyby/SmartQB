using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

public interface IPdfService
{
    /// <summary>
    /// Gets the total number of pages in a PDF file.
    /// </summary>
    int GetPageCount(string filePath);

    /// <summary>
    /// Renders a specific page of a PDF as a PNG image (byte array).
    /// </summary>
    Task<byte[]> RenderPageAsync(string filePath, int pageIndex);

    /// <summary>
    /// Extracts segments of a page into separate question images synchronously.
    /// </summary>
    List<byte[]> ExtractQuestionImages(string filePath, int pageIndex);
}
