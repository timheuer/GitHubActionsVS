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

    public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
    {
        return Task.FromResult<FrameworkElement>(new GHActionsToolWindow());
    }

    [Guid("4a4ad204-3623-4e03-b2d1-6fef94652174")]
    internal class Pane : ToolkitToolWindowPane
    {
        public Pane()
        {
            BitmapImageMoniker = KnownMonikers.ToolWindow;
        }
    }
}