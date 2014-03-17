using Roslyn.Services.Host;

namespace min_cs_editor
{
    public class SimpleWorkspace : TrackingWorkspace
    {
        public SimpleWorkspace(IWorkspaceServiceProvider workspaceServiceProvider, bool enableBackgroundCompilation = true, bool enableInProgressSolutions = true)
            : base(workspaceServiceProvider, enableBackgroundCompilation, enableInProgressSolutions)
        {
        }
    }
}
