using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Roslyn.Compilers;
using Roslyn.Services;

namespace min_cs_editor
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(CompletionItem completionData)
        {
            Text = completionData.DisplayText;
            Content = completionData.DisplayText;
            Description = completionData.GetDescription().ToDisplayString();
            Priority = 0;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var offset = completionSegment.Offset - 1;
            var length = completionSegment.Length + 1;

            textArea.Document.Replace(offset, length, Text);
        }

        public ImageSource Image { get; private set; }
        public string Text { get; private set; }
        public object Content { get; private set; }
        public object Description { get; private set; }
        public double Priority { get; private set; }
    }
}
