﻿global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using GitHubActionsVS.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Runtime.Versioning;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace GitHubActionsVS;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideToolWindow(typeof(ActionsToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
[ProvideOptionPage(typeof(OptionsProvider.ExtensionOptionsOptions), "GitHub", "Actions", 0, 0, true, SupportsProfiles = true)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.GitHubActionsVSString)]
[ProvideBindingPath]
[ProvideService(typeof(ToolWindowMessenger), IsAsyncQueryable = true)]
public sealed class GitHubActionsVSPackage : ToolkitPackage, IVsSolutionEvents
{
    private IVsSolution _solution;
    private uint _cookie;

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        AddService(typeof(ToolWindowMessenger), (_, _, _) => Task.FromResult<object>(new ToolWindowMessenger()));

        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // Load native libsodium for current architecture (x64 or ARM64) from VSIX content
        TryLoadLibSodium();

        _solution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
        _solution?.AdviseSolutionEvents(this, out _cookie);

        await this.RegisterCommandsAsync();

        this.RegisterToolWindows();
    }

    private void TryLoadLibSodium()
    {
        try
        {
            var asmLocation = Path.GetDirectoryName(typeof(GitHubActionsVSPackage).Assembly.Location);
            if (asmLocation is null)
                return;
            var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            string archFolder = arch switch
            {
                System.Runtime.InteropServices.Architecture.X64 => "win-x64",
                System.Runtime.InteropServices.Architecture.Arm64 => "win-arm64",
                _ => null
            };
            if (archFolder is null) return; // unsupported
            string nativePath = Path.Combine(asmLocation, "runtimes", archFolder, "native", "libsodium.dll");
            if (!File.Exists(nativePath)) return; // not present
            IntPtr handle = LoadLibrary(nativePath);
            if (handle == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"Failed to load libsodium from {nativePath}. Win32Error={err}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Exception loading libsodium: " + ex);
        }
    }

    protected override void Dispose(bool disposing)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (_solution is not null)
        {
            _solution.UnadviseSolutionEvents(_cookie);
            _solution = null;
        }
        base.Dispose(disposing);
    }

    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
        if (ActionsToolWindow.Instance is { ActionsWindow: { } } window)
        {
            _ = window.ActionsWindow.GetRepoInfoAsync();
        }

        return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object pUnkReserved)
    {
        if (ActionsToolWindow.Instance is { ActionsWindow: { } } window)
        {
            window.ActionsWindow.ResetTrees();
        }

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