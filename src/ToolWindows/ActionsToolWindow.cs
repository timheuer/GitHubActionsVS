using GitHubActionsVS.ToolWindows;
using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GitHubActionsVS;
public class ActionsToolWindow : BaseToolWindow<ActionsToolWindow>
{
    public override string GetTitle(int toolWindowId) => "GitHub Actions";

    public override Type PaneType => typeof(Pane);

    public GHActionsToolWindow ActionsWindow { get; private set; }
    private static ActionsToolWindow _instance;
    public static ActionsToolWindow Instance => _instance ??= new ActionsToolWindow();

    public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
    {
        ToolWindowMessenger toolWindowMessenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();

        ActionsWindow = new GHActionsToolWindow(toolWindowMessenger);
        _instance = this;
        return ActionsWindow;
    }

    [Guid("4a4ad204-3623-4e03-b2d1-6fef94652174")]
    internal class Pane : ToolkitToolWindowPane
    {
        public Pane()
        {
            BitmapImageMoniker = KnownMonikers.ToolWindow;
            ToolBar = new System.ComponentModel.Design.CommandID(PackageGuids.GitHubActionsVS, PackageIds.TWindowToolbar);
        }
    }
}