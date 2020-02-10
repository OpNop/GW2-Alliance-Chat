using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Chat_Client
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Brush color)
        {
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }
            catch (FormatException) { }
        }
    }
}
