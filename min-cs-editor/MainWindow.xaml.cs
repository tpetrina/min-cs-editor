using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Host;

namespace min_cs_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CompletionWindow _completionWindow;
        private ICompletionService _completionService;
        private SimpleWorkspace _workspace;
        private ISolution _solution;
        private CompilationOptions _compilationOptions;
        private ParseOptions _parseOptions;
        IEnumerable<PortableExecutableReference> _references = new List<PortableExecutableReference>();
        private IProject _project;
        private ISolution _currentSolution;
        private DocumentId _currentDocumentId;

        public MainWindow()
        {
            InitializeComponent();

            Editor.TextArea.TextEntering += TextArea_TextEntering;
            Editor.TextArea.TextEntered += TextAreaOnTextEntered;

            var workspaceServiceProvider = DefaultServices.WorkspaceServicesFactory.CreateWorkspaceServiceProvider("RoslynPad");
            _workspace = new SimpleWorkspace(workspaceServiceProvider);
            _completionService = _workspace
                    .CurrentSolution
                    .LanguageServicesFactory
                    .CreateLanguageServiceProvider(LanguageNames.CSharp)
                    .GetCompletionService();

            _solution = _workspace.CurrentSolution;

            _compilationOptions = new CompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            _parseOptions = new ParseOptions(CompatibilityMode.None, LanguageVersion.CSharp6, true, SourceCodeKind.Script);

            _project = CreateSubmissionProject(_solution);
            _currentSolution = _project.Solution.AddDocument(_project.Id, _project.Name, UsingText, out _currentDocumentId);

            DocumentId id;
            var solution = _project.Solution.AddDocument(_project.Id, _project.Name, Editor.Text, out id);
            CurrentScript = solution.GetDocument(id);
        }

        public IText UsingText
        {
            get
            {
                return new StringText("");
            }
        }

        private IProject CreateSubmissionProject(ISolution solution)
        {
            var name = "New Project";
            var projectId = ProjectId.CreateNewId(solution.Id, name);

            var version = VersionStamp.Create();
            var compilationOptions = _compilationOptions.WithScriptClassName(name);

            var projectInfo = new ProjectInfo(
                projectId,
                version,
                name,
                name,
                LanguageNames.CSharp,
                compilationOptions: compilationOptions,
                parseOptions: _parseOptions,
                metadataReferences: _references,
                isSubmission: true);

            solution = solution.AddProject(projectInfo);

            return solution.GetProject(projectId);
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length <= 0 || _completionWindow == null) return;

            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        public bool IsCompletionTriggerCharacter(int position)
        {
            var currentScriptText = new StringText(Editor.Text);
            var completionProviders = _completionService.GetDefaultCompletionProviders();

            return _completionService.IsTriggerCharacter(currentScriptText, position, completionProviders);
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (Editor.CaretOffset <= 0) return;

            var isTrigger = IsCompletionTriggerCharacter(Editor.CaretOffset - 1);
            if (!isTrigger) return;

            _completionWindow = new CompletionWindow(Editor.TextArea);

            var data = _completionWindow.CompletionList.CompletionData;

            var completion = GetCompletion(Editor.CaretOffset, Editor.Text[Editor.CaretOffset - 1]).ToList();
            if (!completion.Any())
            {
                _completionWindow = null;
                return;
            }

            foreach (var completionData in completion)
            {
                data.Add(new CompletionData(completionData));
            }

            _completionWindow.Show();
            _completionWindow.Closed += (o, args) => _completionWindow = null;
        }

        public IEnumerable<CompletionItem> GetCompletion(int position, char triggerChar)
        {
            var completionTrigger = CompletionTriggerInfo.CreateTypeCharTriggerInfo(triggerChar);
            var completionProviders = _completionService.GetDefaultCompletionProviders();


            var groups = _completionService.GetGroups(
                CurrentScript.UpdateText(new StringText(Editor.Text)),
                position,
                completionTrigger,
                completionProviders,
                CancellationToken.None);

            if (groups == null) yield break;

            foreach (var completionItem in groups.SelectMany(x => x.Items))
            {
                yield return completionItem;
            }
        }

        public IDocument CurrentScript
        {
            get;
            private set;
        }
    }
}
