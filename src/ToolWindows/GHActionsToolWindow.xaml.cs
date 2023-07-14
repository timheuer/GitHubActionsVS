using GitHubActionsVS.Helpers;
using GitHubActionsVS.Models;
using GitHubActionsVS.ToolWindows;
using Octokit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHubActionsVS;

/// <summary>
/// Interaction logic for GHActionsToolWindow.xaml
/// </summary>
public partial class GHActionsToolWindow : UserControl
{
    private readonly RepoInfo _repoInfo = null;
    private readonly ToolWindowMessenger _toolWindowMessenger = null;

    public GHActionsToolWindow(ToolWindowMessenger toolWindowMessenger)
    {
        _toolWindowMessenger = toolWindowMessenger ??= new();
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

    public void ResetTrees() => ClearTreeViews();

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
        tvSecrets.Items.Clear();
        //tvWorkflows.Items.Clear();
        tvCurrentBranch.ItemsSource = null;
        tvEnvironments.Items.Clear();
    }

    private async Task LoadDataAsync()
    {

        // get the settings
        var generalSettings = await ExtensionOptions.GetLiveInstanceAsync();
        var maxRuns = generalSettings.MaxRuns;

        refreshProgress.IsIndeterminate = true;
        refreshProgress.Visibility = Visibility.Visible;

        GitHubClient client = GetGitHubClient();

        try
        {
            // get secrets
            var repoSecrets = await client.Repository?.Actions?.Secrets?.GetAll(_repoInfo.RepoOwner, _repoInfo.RepoName);
            foreach (var secret in repoSecrets.Secrets)
            {
                var updatedOrCreatedAt = secret.UpdatedAt.GetValueOrDefault(secret.CreatedAt);
                var item = new TreeViewItem
                {
                    Header = $"{secret.Name} ({updatedOrCreatedAt:g})",
                    Tag = secret,
                };

                tvSecrets.Items.Add(item);
            }

            // get workflows
            //var workflows = await client.Actions?.Workflows?.List(_repoInfo.RepoOwner, _repoInfo.RepoName);
            //foreach (var workflow in workflows.Workflows)
            //{
            //    var item = new TreeViewItem
            //    {
            //        Header = workflow.Name,
            //        Tag = workflow
            //    };
            //    tvWorkflows.Items.Add(item);
            //}

            // get current branch
            var runs = await client.Actions?.Workflows?.Runs?.List(_repoInfo.RepoOwner, _repoInfo.RepoName, new WorkflowRunsRequest() { Branch = _repoInfo.CurrentBranch }, new ApiOptions() { PageCount = 1, PageSize = maxRuns });
            
            // creating simplified model of the GH info for the treeview
            List<SimpleRun> runsList = new List<SimpleRun>();

            // iterate throught the runs
            foreach (var run in runs.WorkflowRuns)
            {
                SimpleRun simpleRun = new()
                {
                    Conclusion = run.Conclusion.Value.StringValue,
                    Name = run.Name,
                    LogDate = run.UpdatedAt,
                    Id = run.Id.ToString(),
                    RunNumber = run.RunNumber.ToString()
                };

                // get the jobs for the run
                var jobs = await client.Actions.Workflows.Jobs?.List(_repoInfo.RepoOwner, _repoInfo.RepoName, run.Id);

                List<SimpleJob> simpleJobs = new();

                // iterate through the jobs' steps
                foreach (var job in jobs.Jobs)
                {
                    List<SimpleJob> steps = new();
                    foreach (var step in job.Steps)
                    {
                        steps.Add(new SimpleJob()
                        {
                            Conclusion = step.Conclusion.Value.StringValue,
                            Name = step.Name,
                            Url = $"{job.HtmlUrl}#step:{step.Number.ToString()}:1"
                        });
                    }
                    simpleJobs.Add(new SimpleJob()
                    {
                        Conclusion = job.Conclusion.Value.StringValue,
                        Name = job.Name,
                        Id = job.Id.ToString(),
                        Jobs = steps // add the steps to the job
                    });
                }

                // add the jobs to the run
                simpleRun.Jobs = simpleJobs;

                runsList.Add(simpleRun);
            }

            tvCurrentBranch.ItemsSource = runsList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        refreshProgress.Visibility = Visibility.Collapsed;
        refreshProgress.IsIndeterminate = false;
    }

    private UIElement CreateEmojiContent(string emojiString)
    {
        var emojiBlock = new Emoji.Wpf.TextBlock();
        emojiBlock.Text = emojiString;

        return emojiBlock;
    }

    private static GitHubClient GetGitHubClient()
    {
        var creds = CredentialManager.GetCredentials("git:https://github.com");
        var client = new GitHubClient(new ProductHeaderValue("VisualStudio"))
        {
            Credentials = new(creds.Username, creds.Password)
        };

        return client;
    }

    private void JobItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // get the items Tag
        if (sender is TreeViewItem item && item.Header is SimpleJob job && job.Url is not null)
        {
            Process.Start(job.Url);
        }
    }

    private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}

