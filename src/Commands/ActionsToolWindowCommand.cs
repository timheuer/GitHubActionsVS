namespace GitHubActionsVS;

[Command(PackageIds.ActionsCommand)]
internal sealed class ActionsToolWindowCommand : BaseCommand<ActionsToolWindowCommand>
{
    protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        return ActionsToolWindow.ShowAsync();
    }
}
