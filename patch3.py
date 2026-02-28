with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

content = content.replace("namespace SmartQB.Infrastructure.Services;\n\npublic class PdfService : IPdfService\n{\n    private const int GAP_THRESHOLD = 30; // Minimum white space (pixels) to be considered a split\n    private const int NOISE_THRESHOLD = 5; // Allow some noise (5 pixels)\n    private const int MIN_QUESTION_HEIGHT = 50; // Ignore tiny slices (e.g. noise)",
"""using SmartQB.Core.Configuration;
using Microsoft.Extensions.Options;

namespace SmartQB.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly PdfExtractionOptions _options;

    public PdfService(IOptions<PdfExtractionOptions> options)
    {
        _options = options?.Value ?? new PdfExtractionOptions();
    }""")

content = content.replace("NOISE_THRESHOLD", "_options.NoiseThreshold")
content = content.replace("GAP_THRESHOLD", "_options.GapThreshold")
content = content.replace("MIN_QUESTION_HEIGHT", "_options.MinQuestionHeight")

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write(content)
