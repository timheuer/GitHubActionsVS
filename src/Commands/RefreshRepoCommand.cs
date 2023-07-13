using GitHubActionsVS.ToolWindows;

namespace GitHubActionsVS;

[Command(PackageIds.RefreshRepoCommand)]
internal sealed class RefreshRepoCommand : BaseCommand<RefreshRepoCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
            messenger.Send(new(MessageCommand.Refresh));
        }).FireAndForget();
    }
}
