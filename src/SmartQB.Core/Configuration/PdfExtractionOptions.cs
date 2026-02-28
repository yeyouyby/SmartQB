namespace SmartQB.Core.Configuration;

public class PdfExtractionOptions
{
    /// <summary>
    /// Minimum white space (pixels) to be considered a split.
    /// </summary>
    public int GapThreshold { get; set; } = 30;

    /// <summary>
    /// Allow some noise (pixels) before considering a row non-empty.
    /// </summary>
    public int NoiseThreshold { get; set; } = 5;

    /// <summary>
    /// Minimum height for a question segment. Ignores tiny slices (e.g. noise).
    /// </summary>
    public int MinQuestionHeight { get; set; } = 50;
}
