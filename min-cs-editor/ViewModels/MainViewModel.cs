using System;
using ICSharpCode.AvalonEdit.Document;
using Roslyn.Compilers;

namespace min_cs_editor.ViewModels
{
    public class MainViewModel : ViewModelBase, ITextContainer
    {
        #region Backing fields for the bindable properties
        private TextDocument _document;
        private IText _oldText;
        #endregion;

        #region Bindable properties
        public IText CurrentText { get; private set; }

        public TextDocument Document
        {
            get { return _document; }
            set
            {
                if (Equals(value, _document)) return;
                _document = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MainViewModel()
        {
            CurrentText = new StringText(string.Empty);

            Document = new TextDocument(string.Empty);
            Document.Changing += Document_Changing;
            Document.Changed += Document_Changed;
        }

        private void Document_Changing(object sender, DocumentChangeEventArgs documentChangeEventArgs)
        {
            _oldText = CurrentText;
        }

        void Document_Changed(object sender, DocumentChangeEventArgs e)
        {
            CurrentText = new StringText(Document.Text);
            OnTextChanged(new TextChangeEventArgs(_oldText, CurrentText, new TextChangeRange[0]));
        }

        #region ITextContainer implementation
        public event EventHandler<TextChangeEventArgs> TextChanged;
        protected virtual void OnTextChanged(TextChangeEventArgs e)
        {
            var handler = TextChanged;
            if (handler != null) handler(this, e);
        }
        #endregion
    }
}
