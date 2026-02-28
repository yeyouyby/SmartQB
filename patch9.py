with open('src/SmartQB.UI/App.xaml.cs', 'r') as f:
    content = f.read()

content = content.replace("                // PDF Service\n                services.AddSingleton<IIngestionService, IngestionService>();",
"""                // Configuration
                services.Configure<SmartQB.Core.Configuration.PdfExtractionOptions>(context.Configuration.GetSection("PdfExtractionOptions"));

                // PDF Service
                services.AddSingleton<IIngestionService, IngestionService>();""")

with open('src/SmartQB.UI/App.xaml.cs', 'w') as f:
    f.write(content)
