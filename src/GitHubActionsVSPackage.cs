﻿global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using GitHubActionsVS.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace GitHubActionsVS;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideToolWindow(typeof(ActionsToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.GitHubActionsVSString)]
[ProvideBindingPath]
[ProvideService(typeof(ToolWindowMessenger), IsAsyncQueryable = true)]
public sealed class GitHubActionsVSPackage : ToolkitPackage, IVsSolutionEvents
{
    private IVsSolution solution;
    private uint cookie;

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        AddService(typeof(ToolWindowMessenger), (_, _, _) => Task.FromResult<object>(new ToolWindowMessenger()));

        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        solution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
        if (solution is not null)
        {
            solution.AdviseSolutionEvents(this, out cookie);
        }

        await this.RegisterCommandsAsync();

        this.RegisterToolWindows();
    }

    protected override void Dispose(bool disposing)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (solution is not null)
        {
            solution.UnadviseSolutionEvents(cookie);
            solution = null;
        }
        base.Dispose(disposing);
    }

    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
        //_ = ActionsToolWindow.Instance?.ActionsWindow?.GetRepoInfoAsync();
        return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object pUnkReserved)
    {
        ActionsToolWindow.Instance.ActionsWindow.ResetTrees();

        return VSConstants.S_OK;
    }

    // Implement other IVsSolutionEvents methods with empty bodies
    public int OnAfterCloseSolution(object pUnkReserved) => VSConstants.S_OK;
    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.S_OK;
    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.S_OK;
    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.S_OK;
    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.S_OK;
    public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.S_OK;
    public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;
    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.S_OK;
}