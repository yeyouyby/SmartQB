using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace SmartQB.UI.Helpers;

public static class WebView2Helper
{
    public static async Task<CoreWebView2Environment> GetEnvironmentAsync()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userDataFolder = Path.Combine(appDataPath, "SmartQB", "WebView2");

        if (!Directory.Exists(userDataFolder))
        {
            Directory.CreateDirectory(userDataFolder);
        }

        return await CoreWebView2Environment.CreateAsync(null, userDataFolder);
    }
}
