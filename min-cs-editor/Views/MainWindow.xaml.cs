using ICSharpCode.AvalonEdit.CodeCompletion;
using min_cs_editor.Completion;
using min_cs_editor.ViewModels;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using roslyn_completion;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace min_cs_editor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CompletionWindow _completionWindow;

        private readonly SimpleWorkspace _workspace;

        private readonly ICompletionService _completionService;
        private readonly CompilationOptions _compilationOptions;
        private readonly ParseOptions _parseOptions;
        readonly IEnumerable<PortableExecutableReference> _references = new List<PortableExecutableReference>();
        private readonly ISolution _currentSolution;
        private readonly DocumentId _currentDocumentId;

        public MainWindow()
        {
            MainViewModel vm;
            InitializeComponent();

            DataContext = vm = new MainViewModel();

            Editor.TextArea.TextEntering += TextArea_TextEntering;
            Editor.TextArea.TextEntered += TextAreaOnTextEntered;

            var workspaceServiceProvider = DefaultServices.WorkspaceServicesFactory.CreateWorkspaceServiceProvider("RoslynPad");
            _workspace = new SimpleWorkspace(workspaceServiceProvider);
            _completionService = _workspace
                    .CurrentSolution
                    .LanguageServicesFactory
                    .CreateLanguageServiceProvider(LanguageNames.CSharp)
                    .GetCompletionService();

            var solution = _workspace.CurrentSolution;

            _compilationOptions = new CompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            _parseOptions = new ParseOptions(CompatibilityMode.None, LanguageVersion.CSharp6, true, SourceCodeKind.Script);

            var project = CreateSubmissionProject(solution);
            _currentSolution = project.Solution.AddDocument(project.Id, project.Name, UsingText, out _currentDocumentId);

            DocumentId id;
            _currentSolution = project.Solution.AddDocument(project.Id, project.Name, vm.CurrentText, out id);
            var doc = _currentSolution.GetDocument(id);

            _currentDocumentId = doc.Id;

            _workspace.SetCurrentSolution(_currentSolution);
            _workspace.OpenDocument(_currentDocumentId, vm);
        }

        public IDocument CurrentScript
        {
            get { return _currentSolution.GetDocument(_currentDocumentId); }
        }

        public IText UsingText
        {
            get { return new StringText(""); }
        }

        private IProject CreateSubmissionProject(ISolution solution)
        {
            const string name = "New Project";
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
    }
}
