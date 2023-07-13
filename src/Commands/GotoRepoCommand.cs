using GitHubActionsVS.ToolWindows;

namespace GitHubActionsVS;

[Command(PackageIds.GotoRepoCommand)]
internal sealed class GotoRepoCommand : BaseCommand<GotoRepoCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
            messenger.Send(new(MessageCommand.GotoRepo));
        }).FireAndForget();
    }
}
