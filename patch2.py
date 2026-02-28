with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

content = content.replace("                using var imageSubset = SKImage.FromBitmap(subset);\n                using var dataSubset = imageSubset.Encode(SKEncodedImageFormat.Png, 100);\n                results.Add(dataSubset.ToArray());\n            }\n\nreturn results;", "            using var imageSubset = SKImage.FromBitmap(subset);\n            using var dataSubset = imageSubset.Encode(SKEncodedImageFormat.Png, 100);\n            results.Add(dataSubset.ToArray());\n        }\n\n        return results;")

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write(content)
