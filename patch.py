import re

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'r') as f:
    content = f.read()

# Replace ExtractQuestionImagesAsync with ExtractQuestionImages
content = content.replace("public Task<List<byte[]>> ExtractQuestionImagesAsync(string filePath, int pageIndex)", "public List<byte[]> ExtractQuestionImages(string filePath, int pageIndex)")

# Remove Task.Run(() => { and replace return results; }); with return results;
content = content.replace("return Task.Run(() =>\n        {\n            var results = new List<byte[]>();", "var results = new List<byte[]>();")

content = content.replace("            return results;\n        });", "return results;")

# Fix indentation. The block is indented by an extra 4 spaces
lines = content.split('\n')
for i in range(len(lines)):
    if i >= 31 and i <= 90:
        if lines[i].startswith('            '):
            lines[i] = lines[i][4:]

with open('src/SmartQB.Infrastructure/Services/PdfService.cs', 'w') as f:
    f.write('\n'.join(lines))
