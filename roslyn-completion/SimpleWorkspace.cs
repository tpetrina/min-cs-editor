using Roslyn.Services.Host;

namespace roslyn_completion
{
    public class SimpleWorkspace : TrackingWorkspace
    {
        public SimpleWorkspace(IWorkspaceServiceProvider workspaceServiceProvider, bool enableBackgroundCompilation = true, bool enableInProgressSolutions = true)
            : base(workspaceServiceProvider, enableBackgroundCompilation, enableInProgressSolutions)
        {
        }
    }
}
