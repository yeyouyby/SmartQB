with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

old_loop = """        // Convert to grayscale for analysis to simplify thresholding
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
        }"""

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

content = content.replace(old_loop, new_loop)

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write(content)
