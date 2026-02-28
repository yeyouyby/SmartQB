with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

content = content.replace("        ReadOnlySpan<SKColor> pixels = bitmap.Pixels;", "        ReadOnlySpan<SKColor> pixels = bitmap.GetPixelSpan();")

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write(content)
