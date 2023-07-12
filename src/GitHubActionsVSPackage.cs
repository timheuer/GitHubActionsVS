global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using GitHubActionsVS.ToolWindows;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace GitHubActionsVS;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideToolWindow(typeof(ActionsToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.GitHubActionsVSString)]
[ProvideBindingPath]
[ProvideService(typeof(ToolWindowMessenger), IsAsyncQueryable = true)]
public sealed class GitHubActionsVSPackage : ToolkitPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        AddService(typeof(ToolWindowMessenger), (_, _, _) => Task.FromResult<object>(new ToolWindowMessenger()));

        await this.RegisterCommandsAsync();

        this.RegisterToolWindows();
    }
}