with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

new_loop = """        // Convert to grayscale for analysis to simplify thresholding
        var pixels = bitmap.GetPixelSpan();
        for (int y = 0; y < height; y++)
        {
            int inkCount = 0;
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                var color = pixels[rowOffset + x];
                // Simple luminance calculation or just check if not white
                if (color.Red < 240 || color.Green < 240 || color.Blue < 240)
                {
                    inkCount++;
                }
            }
            rowInkDensity[y] = inkCount;
        }"""

fixed_loop = """        // Convert to grayscale for analysis to simplify thresholding
        ReadOnlySpan<SKColor> pixels = bitmap.Pixels;
        for (int y = 0; y < height; y++)
        {
            int inkCount = 0;
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                var color = pixels[rowOffset + x];
                // Simple luminance calculation or just check if not white
                if (color.Red < 240 || color.Green < 240 || color.Blue < 240)
                {
                    inkCount++;
                }
            }
            rowInkDensity[y] = inkCount;
        }"""

content = content.replace(new_loop, fixed_loop)

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write(content)
