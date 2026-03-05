using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfMath.Controls;

namespace SmartQB.UI.Helpers;

public static class MathRenderingHelper
{
    public static readonly DependencyProperty MathTextProperty =
        DependencyProperty.RegisterAttached("MathText", typeof(string), typeof(MathRenderingHelper), new PropertyMetadata(string.Empty, OnMathTextChanged));

    public static string GetMathText(DependencyObject obj)
    {
        return (string)obj.GetValue(MathTextProperty);
    }

    public static void SetMathText(DependencyObject obj, string value)
    {
        obj.SetValue(MathTextProperty, value);
    }

    private static void OnMathTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel)
        {
            panel.Children.Clear();

            if (!(e.NewValue is string text) || string.IsNullOrWhiteSpace(text))
                return;

            // Simple regex to match LaTeX formulas wrapped in $...$
            var regex = new Regex(@"\$(.*?)\$", RegexOptions.None, TimeSpan.FromMilliseconds(500));
            var matches = regex.Matches(text);

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                // Add plain text before the math
                if (match.Index > lastIndex)
                {
                    string plainText = text.Substring(lastIndex, match.Index - lastIndex);
                    panel.Children.Add(new TextBlock { Text = plainText, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
                }

                // Add math formula
                string formula = match.Groups[1].Value;
                try
                {
                    var formulaControl = new FormulaControl { Formula = formula, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 2, 0) };
                    panel.Children.Add(formulaControl);
                }
                catch
                {
                    // Fallback to plain text if rendering fails
                    panel.Children.Add(new TextBlock { Text = $"${formula}$", VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
                }

                lastIndex = match.Index + match.Length;
            }

            // Add remaining plain text
            if (lastIndex < text.Length)
            {
                string plainText = text.Substring(lastIndex);
                panel.Children.Add(new TextBlock { Text = plainText, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
            }
        }
    }
}
