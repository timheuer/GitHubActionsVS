using GitHubActionsVS.Helpers;
using GitHubActionsVS.ToolWindows;
using LibGit2Sharp;
using Octokit;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GitHubActionsVS;
/// <summary>
/// Interaction logic for GHActionsToolWindow.xaml
/// </summary>
public partial class GHActionsToolWindow : UserControl
{
    RepoInfo repoInfo = null;
    public ToolWindowMessenger ToolWindowMessenger = null;

    public GHActionsToolWindow(ToolWindowMessenger toolWindowMessenger)
    {
        if (toolWindowMessenger == null)
        {
            toolWindowMessenger = new();
        }
        ToolWindowMessenger = toolWindowMessenger;
        toolWindowMessenger.MessageReceived += OnMessageReceived;
        InitializeComponent();
        repoInfo = new();

        _ = GetRepoInfoAsync();
    }

    private void OnMessageReceived(object sender, string e)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            switch (e)
            {
                case "RefreshCommand Message":
                    await GetRepoInfoAsync();
                    break;
                case "GotoRepoCommand Message":
                    await GotoRepoAsync();
                    break;
            }
        }).FireAndForget();
    }

    private async Task GotoRepoAsync()
    {
        Process.Start(repoInfo?.RepoUrl);
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

        repoInfo.FindGitFolder(projectPath, out string gitPath);

        if (string.IsNullOrEmpty(gitPath))
        {
            Debug.WriteLine("No git repo found");
            return;
        }
        else
        {
            Debug.WriteLine($"Found git repo at {gitPath}");
            // if not a github repo, bail
            if (!repoInfo.IsGitHub)
            {
                Debug.WriteLine("Not a GitHub repo");
                return;
            }
            else
            {
                Debug.WriteLine($"GitHub repo: {repoInfo.RepoOwner}/{repoInfo.RepoName}");
                // load the data
                await LoadDataAsync();
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
        Octokit.Credentials ghCreds = new Octokit.Credentials(creds.Username, creds.Password);
        github.Credentials = ghCreds;

        var style1 = this.TryFindResource("EmojiTreeViewItem") as Style;

        if (style1 is null)
        {
            Debug.WriteLine("did not find style");
        }

        try
        {
            // get secrets
            var repoSecrets = await github.Repository?.Actions?.Secrets?.GetAll(repoInfo.RepoOwner, repoInfo.RepoName);
            foreach (var secret in repoSecrets.Secrets)
            {
                var item = new TreeViewItem();
                item.Header = secret.Name;
                item.Tag = secret;
                tvSecrets.Items.Add(item);
            }

            // get workflows
            var workflows = await github.Actions?.Workflows?.List(repoInfo.RepoOwner, repoInfo.RepoName);
            foreach (var workflow in workflows.Workflows)
            {
                var item = new TreeViewItem();
                item.Header = workflow.Name;
                item.Tag = workflow;
                tvWorkflows.Items.Add(item);
            }

            // get current branch
            var runs = await github.Actions?.Workflows?.Runs?.List(repoInfo.RepoOwner, repoInfo.RepoName, new WorkflowRunsRequest() { Branch = repoInfo.CurrentBranch }, new ApiOptions() { PageCount = 2, PageSize = 10 });
            foreach (var run in runs.WorkflowRuns)
            {
                var item = new TreeViewItem();
                item.Style = style1;
                item.Header = $"{GetConclusionIndicator(run.Conclusion.Value.StringValue.ToString())} {run.Name} #{run.RunNumber}";
                item.Tag = run;

                // iterate through the run
                var jobs = await github.Actions?.Workflows?.Jobs?.List(repoInfo.RepoOwner, repoInfo.RepoName, run.Id);
                foreach (var job in jobs.Jobs)
                {
                    var jobItem = new TreeViewItem();
                    jobItem.Style = style1;
                    jobItem.Header = $"{GetConclusionIndicator(job.Conclusion.Value.StringValue.ToString())} {job.Name}";
                    jobItem.Tag = job;

                    // iterate through the job          
                    foreach (var step in job.Steps)
                    {
                        var stepItem = new TreeViewItem();
                        stepItem.Style = style1;
                        stepItem.Header = $"{GetConclusionIndicator(step.Conclusion.Value.StringValue.ToString())}: {step.Name}";
                        stepItem.Tag = step;
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

    private string GetConclusionIndicator(string status)
    {

        switch (status.ToLowerInvariant())
        {
            case "success":
                return "✅";
            case "failure":
                return "❌";
            case "cancelled":
                return "🚫";
            case "skipped":
                return "⏭";
            default:
                return "🤷‍♂️";
        }
    }

    private void JobItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        throw new NotImplementedException();
    }
}

