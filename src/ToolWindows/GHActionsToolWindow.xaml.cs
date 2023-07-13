using GitHubActionsVS.Helpers;
using GitHubActionsVS.ToolWindows;
using Octokit;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GitHubActionsVS;

/// <summary>
/// Interaction logic for GHActionsToolWindow.xaml
/// </summary>
public partial class GHActionsToolWindow : UserControl
{
    readonly RepoInfo _repoInfo = null;

    public ToolWindowMessenger ToolWindowMessenger = null;

    public GHActionsToolWindow(ToolWindowMessenger toolWindowMessenger)
    {
        toolWindowMessenger ??= new();
        ToolWindowMessenger = toolWindowMessenger;
        toolWindowMessenger.MessageReceived += OnMessageReceived;
        InitializeComponent();
        _repoInfo = new();

        _ = GetRepoInfoAsync();
    }

    private void OnMessageReceived(object sender, MessagePayload payload)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            var (command, text) = payload;
            await (command switch
            {
                MessageCommand.Refresh => GetRepoInfoAsync(),
                MessageCommand.GotoRepo => GotoRepoAsync(),

                _ => Task.CompletedTask
            });
        }).FireAndForget();
    }

    private Task GotoRepoAsync()
    {
        if (_repoInfo is { RepoUrl.Length: > 0 })
        {
            Process.Start(_repoInfo?.RepoUrl);
        }

        return Task.CompletedTask;
    }

    public void ResetTrees()
    {
        ClearTreeViews();
    }

    public async Task GetRepoInfoAsync()
    {
        ClearTreeViews();

        // find the git folder
        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        var projectPath = solution?.FullPath;

        _repoInfo.FindGitFolder(projectPath, out string gitPath);

        if (string.IsNullOrWhiteSpace(gitPath))
        {
            Debug.WriteLine("No git repo found");
        }
        else
        {
            Debug.WriteLine($"Found git repo at {gitPath}");
            if (_repoInfo.IsGitHub)
            {
                Debug.WriteLine($"GitHub repo: {_repoInfo.RepoOwner}/{_repoInfo.RepoName}");
                // load the data
                await LoadDataAsync();
            }
            else
            {
                Debug.WriteLine("Not a GitHub repo");
            }
        }
    }

    private void ClearTreeViews()
    {
        // clear out the treeviews
        tvSecrets.Items.Clear();
        tvWorkflows.Items.Clear();
        tvCurrentBranch.Items.Clear();
        tvEnvironments.Items.Clear();
    }

    private async Task LoadDataAsync()
    {
        var creds = CredentialManager.GetCredentials("git:https://github.com");
        var github = new GitHubClient(new ProductHeaderValue("VisualStudio"));
        //var token = Environment.GetEnvironmentVariable("GITHUB_PAT_VS");
        //Credentials ghCreds = new Credentials(token);
        Credentials ghCreds = new(creds.Username, creds.Password);
        github.Credentials = ghCreds;

        var style1 = TryFindResource("EmojiTreeViewItem") as Style;

        if (style1 is null)
        {
            Debug.WriteLine("did not find style");
        }

        try
        {
            // get secrets
            var repoSecrets = await github.Repository?.Actions?.Secrets?.GetAll(_repoInfo.RepoOwner, _repoInfo.RepoName);
            foreach (var secret in repoSecrets.Secrets)
            {
                var item = new TreeViewItem
                {
                    Header = secret.Name,
                    Tag = secret
                };
                tvSecrets.Items.Add(item);
            }

            // get workflows
            var workflows = await github.Actions?.Workflows?.List(_repoInfo.RepoOwner, _repoInfo.RepoName);
            foreach (var workflow in workflows.Workflows)
            {
                var item = new TreeViewItem
                {
                    Header = workflow.Name,
                    Tag = workflow
                };
                tvWorkflows.Items.Add(item);
            }

            // get current branch
            var runs = await github.Actions?.Workflows?.Runs?.List(_repoInfo.RepoOwner, _repoInfo.RepoName, new WorkflowRunsRequest() { Branch = _repoInfo.CurrentBranch }, new ApiOptions() { PageCount = 2, PageSize = 10 });
            foreach (var run in runs.WorkflowRuns)
            {
                var item = new TreeViewItem
                {
                    Style = style1,
                    Header = $"{GetConclusionIndicator(run.Conclusion.Value.StringValue.ToString())} {run.Name} #{run.RunNumber}",
                    Tag = run
                };

                // iterate through the run
                var jobs = await github.Actions?.Workflows?.Jobs?.List(_repoInfo.RepoOwner, _repoInfo.RepoName, run.Id);
                foreach (var job in jobs.Jobs)
                {
                    var jobItem = new TreeViewItem
                    {
                        Style = style1,
                        Header = $"{GetConclusionIndicator(job.Conclusion.Value.StringValue.ToString())} {job.Name}",
                        Tag = job
                    };

                    // iterate through the job          
                    foreach (var step in job.Steps)
                    {
                        var stepItem = new TreeViewItem
                        {
                            Style = style1,
                            Header = $"{GetConclusionIndicator(step.Conclusion.Value.StringValue.ToString())}: {step.Name}",
                            Tag = step
                        };
                        jobItem.Items.Add(stepItem);
                    }

                    item.Items.Add(jobItem);
                }
                tvCurrentBranch.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private string GetConclusionIndicator(string status) => status.ToLowerInvariant() switch
    {
        "success" => "✅",
        "failure" => "❌",
        "cancelled" => "🚫",
        "skipped" => "⏭",
        _ => "🤷🏽",
    };

    private void JobItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        throw new NotImplementedException();
    }
}

