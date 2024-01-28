using GitHubActionsVS.ToolWindows;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubActionsVS;

[Command(PackageIds.ReportFeedbackCommand)]
internal sealed class ReportFeedbackCommand : BaseCommand<ReportFeedbackCommand>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();

            var vsVersion = await VS.Shell.GetVsVersionAsync();

            messenger.Send(new(MessageCommand.ReportFeedback, vsVersion.ToString()));
        }).FireAndForget();
    }
}