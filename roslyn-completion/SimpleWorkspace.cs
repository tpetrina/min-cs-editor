using Roslyn.Compilers;
using Roslyn.Services;
using Roslyn.Services.Host;

namespace roslyn_completion
{
    public class SimpleWorkspace : TrackingWorkspace
    {
        public SimpleWorkspace(IWorkspaceServiceProvider workspaceServiceProvider, bool enableBackgroundCompilation = true, bool enableInProgressSolutions = true)
            : base(workspaceServiceProvider, enableBackgroundCompilation, enableInProgressSolutions)
        {
        }

        public void SetCurrentSolution(ISolution solution)
        {
            SetLatestSolution(solution);
            RaiseWorkspaceChangedEventAsync(WorkspaceEventKind.SolutionChanged, solution);
        }

        public void OpenDocument(DocumentId documentId, ITextContainer textContainer)
        {
            OnDocumentOpened(documentId, textContainer);
        }
    }
}
