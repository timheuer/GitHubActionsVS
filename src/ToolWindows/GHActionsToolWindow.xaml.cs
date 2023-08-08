using GitHubActionsVS.Helpers;
using GitHubActionsVS.Models;
using GitHubActionsVS.ToolWindows;
using Octokit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHubActionsVS.UserControls;
using Application = System.Windows.Application;
using System.Windows.Media;
using MessageBox = Community.VisualStudio.Toolkit.MessageBox;
using resx = GitHubActionsVS.Resources.UIStrings;
using Humanizer;

namespace GitHubActionsVS;

/// <summary>
/// Interaction logic for GHActionsToolWindow.xaml
/// </summary>
public partial class GHActionsToolWindow : UserControl
{
    private readonly RepoInfo _repoInfo = null;
    private readonly ToolWindowMessenger _toolWindowMessenger = null;
    private int maxRuns = 10;
    private bool refreshPending = false;
    private int refreshInterval = 5;
    OutputWindowPane _pane;

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
            Process.Start($"{_repoInfo?.RepoUrl}/actions");
        }

        return Task.CompletedTask;
    }

    public void ResetTrees() => ClearTreeViews();

    public async Task GetRepoInfoAsync()
    {
        if (_pane is null)
        {
            _pane = await VS.Windows.CreateOutputWindowPaneAsync("GitHub Actions for VS");
        }

        ClearTreeViews();
        _repoInfo.RepoOwner = null;
        _repoInfo.RepoName = null;
        _repoInfo.IsGitHub = false;
        _repoInfo.RepoUrl = null;

        // get settings
        var generalSettings = await ExtensionOptions.GetLiveInstanceAsync();
        maxRuns = generalSettings.MaxRuns;
        refreshInterval = generalSettings.RefreshInterval;
        refreshPending = generalSettings.RefreshActiveJobs;

        await _pane.WriteLineAsync("Extension settings retrieved and applied");

        // find the git folder
        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        if (solution is null)
        {
            await _pane.WriteLineAsync("No solution found");
            Debug.WriteLine("No solution found");
            ShowInfoMessage(resx.NO_PROJ_LOADED);
            return;
        }
        var projectPath = solution?.FullPath;

        _repoInfo.FindGitFolder(projectPath, out string gitPath);

        if (string.IsNullOrWhiteSpace(gitPath))
        {
            await _pane.WriteLineAsync("No git repo found");
            Debug.WriteLine("No git repo found");
            ShowInfoMessage(resx.NO_GIT_REPO);
        }
        else
        {
            Debug.WriteLine($"Found git repo at {gitPath}");
            if (_repoInfo.IsGitHub)
            {
                Debug.WriteLine($"GitHub repo: {_repoInfo.RepoOwner}/{_repoInfo.RepoName}");
                await _pane.WriteLineAsync($"Found repo for {gitPath} at {_repoInfo.RepoOwner}/{_repoInfo.RepoName}");
                await LoadDataAsync();
            }
            else
            {
                await _pane.WriteLineAsync("Not a GitHub repo");
                Debug.WriteLine("Not a GitHub repo");
                ShowInfoMessage(resx.GIT_NOT_GITHUB);
            }
        }
    }

    private void ShowInfoMessage(string messageString)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            MessageArea.Text = messageString;
            MessageArea.Visibility = Visibility.Visible;
            ActionsInfoPanel.Visibility = Visibility.Collapsed;
        });
    }

    private void ClearTreeViews()
    {
        tvSecrets.ItemsSource = null;
        tvSecrets.Header = resx.HEADER_REPO_SECRETS;
        tvEnvironments.ItemsSource = null;
        tvEnvironments.Header = resx.HEADER_ENVIRONMENTS;
        tvCurrentBranch.ItemsSource = null;
        tvWorkflows.ItemsSource = null;
        CurrentBranchExpander.IsExpanded = false;
    }

    private async Task LoadDataAsync()
    {
        MessageArea.Visibility = Visibility.Collapsed;
        ActionsInfoPanel.Visibility = Visibility.Visible;

        refreshProgress.IsIndeterminate = true;
        refreshProgress.Visibility = Visibility.Visible;

        GitHubClient client = GetGitHubClient();

        try
        {
            // get secrets
            await RefreshSecretsAsync(client);
            // get environments
            await RefreshEnvironmentsAsync(client);
            // get workflows
            await RefreshWorkflowsAsync(client);

            await _pane.WriteLineAsync("Loading Workflow Runs...");
            // get current branch
            var runs = await client.Actions?.Workflows?.Runs?.List(_repoInfo.RepoOwner, _repoInfo.RepoName, new WorkflowRunsRequest() { Branch = _repoInfo.CurrentBranch }, new ApiOptions() { PageCount = 1, PageSize = maxRuns });

            List<SimpleRun> runsList = new List<SimpleRun>();

            if (runs.TotalCount > 0)
            {
                await _pane.WriteLineAsync($"Number of runs found: {runs.TotalCount}");
                // creating simplified model of the GH info for the treeview

                // iterate throught the runs
                foreach (var run in runs.WorkflowRuns)
                {
                    SimpleRun simpleRun = new()
                    {
                        Conclusion = run.Conclusion is not null ? run.Conclusion.Value.StringValue.Humanize(LetterCasing.Title) : run.Status.StringValue.Humanize(LetterCasing.Title),
                        Name = run.Name,
                        LogDate = run.UpdatedAt,
                        Id = run.Id.ToString(),
                        RunNumber = run.RunNumber.ToString(),
                        TriggerEvent = run.Event,
                        TriggerLogin = run.TriggeringActor.Login,
                        RunDuration = (run.UpdatedAt - run.RunStartedAt).Humanize(2)
                    };

                    if (refreshPending)
                    {
                        var timer = new System.Timers.Timer(refreshInterval*1000);
                        timer.Elapsed += async (sender, e) =>
                        {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            await LoadDataAsync();
                        };
                        timer.AutoReset = false;

                        if (((run.Status == WorkflowRunStatus.Queued) || (run.Status == WorkflowRunStatus.InProgress) || (run.Status == WorkflowRunStatus.Pending) || (run.Status == WorkflowRunStatus.Waiting)))
                        {
                            timer.Start();
                        }
                    }

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
                                Conclusion = step.Conclusion is not null ? step.Conclusion.Value.StringValue : step.Status.StringValue,
                                Name = step.Name,
                                Url = $"{job.HtmlUrl}#step:{step.Number.ToString()}:1"
                            });
                        }
                        simpleJobs.Add(new SimpleJob()
                        {
                            Conclusion = job.Conclusion is not null ? job.Conclusion.Value.StringValue : job.Status.StringValue,
                            Name = job.Name,
                            Id = job.Id.ToString(),
                            Jobs = steps // add the steps to the job
                        });
                    }

                    // add the jobs to the run
                    simpleRun.Jobs = simpleJobs;

                    runsList.Add(simpleRun);
                }
            }
            else
            {
                // no runs found
                var noRunsItem = new SimpleRun
                {
                    Name = resx.NO_WORKFLOW_RUNS,
                    Conclusion = "warning",
                    LogDate = DateTime.Now,
                    RunNumber = "N/A"
                };
                runsList.Add(noRunsItem);
            }

            tvCurrentBranch.ItemsSource = runsList;
        }
        catch (ApiException ex)
        {
            await _pane.WriteLineAsync($"Error retrieving Workflow Runs: {ex.Message}:{ex.StatusCode}");
            await ex.LogAsync();
        }
        catch (Exception ex)
        {
            await ex.LogAsync();
        }

        CurrentBranchExpander.IsExpanded = true;
        refreshProgress.Visibility = Visibility.Hidden;
        refreshProgress.IsIndeterminate = false;
    }

    private async Task RefreshEnvironmentsAsync(GitHubClient client)
    {
        List<SimpleEnvironment> envList = new List<SimpleEnvironment>();
        await _pane.WriteLineAsync("Loading Environments...");
        try
        {
            var repoEnvs = await client.Repository?.Environment?.GetAll(_repoInfo.RepoOwner, _repoInfo.RepoName);
            
            if (repoEnvs.TotalCount > 0)
            {
                tvEnvironments.Header = $"{resx.HEADER_ENVIRONMENTS} ({repoEnvs.TotalCount})";
                foreach (var env in repoEnvs.Environments)
                {
                    var envItem = new SimpleEnvironment
                    {
                        Name = env.Name,
                        Url = env.HtmlUrl
                    };

                    envList.Add(envItem);
                }
            }
            else
            {
                envList.Add(new() { Name = resx.NO_ENV });
            }
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                envList.Add(new SimpleEnvironment() { Name = resx.INSUFFICIENT_SECRET_PERMS });
                await ex.LogAsync(ex.Message);
            }
        }
        catch (Exception ex)
        {
            envList.Add(new SimpleEnvironment() { Name = "Unable to retrieve Environments, please check logs" });
            await ex.LogAsync();
        }

        tvEnvironments.ItemsSource = envList;
    }

    private async Task RefreshWorkflowsAsync(GitHubClient client)
    {
        await _pane.WriteLineAsync("Loading Workflows...");
        try
        {
            var workflows = await client.Actions?.Workflows?.List(_repoInfo.RepoOwner, _repoInfo.RepoName);
            tvWorkflows.ItemsSource = workflows.Workflows;
        }
        catch (ApiException ex)
        {
            await _pane.WriteLineAsync($"Error retrieving Workflows: {ex.Message}:{ex.StatusCode}");
            await ex.LogAsync();
        }
        catch (Exception ex)
        {
            await ex.LogAsync();
        }

    }
    private async Task RefreshSecretsAsync(GitHubClient client)
    {
        List<string> secretList = new();
        await _pane.WriteLineAsync("Loading Secrets...");
        try
        {
            var repoSecrets = await client.Repository?.Actions?.Secrets?.GetAll(_repoInfo.RepoOwner, _repoInfo.RepoName);

            if (repoSecrets.TotalCount > 0)
            {
                await _pane.WriteLineAsync($"Number of Repository Secrets found: {repoSecrets.TotalCount}");
                tvSecrets.Header = $"{resx.HEADER_REPO_SECRETS} ({repoSecrets.TotalCount})";
                foreach (var secret in repoSecrets.Secrets)
                {
                    var updatedOrCreatedAt = secret.UpdatedAt.GetValueOrDefault(secret.CreatedAt);
                    secretList.Add($"{secret.Name} ({updatedOrCreatedAt:g})");
                }
            }
            else
            {
                tvSecrets.Header = resx.HEADER_REPO_SECRETS;
                secretList.Add(resx.NO_REPO_SECRETS);
            }
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await _pane.WriteLineAsync($"Error retrieving Secrets: {ex.Message}:{ex.StatusCode}");
                secretList.Add(resx.INSUFFICIENT_SECRET_PERMS);
                await ex.LogAsync(ex.Message);
            }
        }
        catch (Exception ex)
        {
            // check to see if a permission thing
            secretList.Add("Unable to retrieve Secrets, please check logs");
            await ex.LogAsync();
        }

        tvSecrets.ItemsSource = secretList;
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

    private async void AddSecret_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await UpsertRepositorySecret(string.Empty);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await _pane.WriteLineAsync($"Error saving Secret: {ex.Message}:{ex.StatusCode}");
                await ex.LogAsync(ex.Message);
            }
        }
        catch (Exception ex)
        {
            await ex.LogAsync();
        }
    }

    private async void EditSecret_Click(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TextBlock tvi = GetParentTreeViewItem(menuItem);
        if (tvi is not null && !tvi.Text.ToLowerInvariant().Contains(" (")) // yes a hack
        {
            string header = tvi.Text.ToString();
            string secretName = header.Substring(0, header.IndexOf(" ("));
            await UpsertRepositorySecret(secretName);
        }
    }

    private TextBlock GetParentTreeViewItem(MenuItem menuItem)
    {
        var contextMenu = menuItem.CommandParameter as ContextMenu;
        if (contextMenu is not null)
        {
            var treeViewItem = contextMenu.PlacementTarget as TextBlock;
            if (treeViewItem is not null)
            {
                return treeViewItem;
            }
        }
        return null;
    }

    private async void DeleteSecret_Click(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TextBlock tvi = GetParentTreeViewItem(menuItem);

        if (tvi is not null && !tvi.Text.ToLowerInvariant().Contains(" (")) // yes a hack
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            // confirm the delete first
            int result = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, resx.CONFIRM_DELETE, resx.CONFIRM_DELETE_TITLE, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_QUERY, Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD);

            var confirmResult = (MessageBoxResult)result;

            if (confirmResult == MessageBoxResult.Yes)
            {
                string header = tvi.Text.ToString();
                string secretName = header.Substring(0, header.IndexOf(" ("));

                GitHubClient client = GetGitHubClient();
                await client.Repository.Actions.Secrets.Delete(_repoInfo.RepoOwner, _repoInfo.RepoName, secretName);
                await RefreshSecretsAsync(client);
            }
        }
    }

    private async Task UpsertRepositorySecret(string secretName)
    {
        AddEditSecret addEditSecret = new AddEditSecret(secretName)
        {
            Owner = Application.Current.MainWindow
        };
        bool? result = addEditSecret.ShowDialog();
        if (result == true)
        {
            GitHubClient client = GetGitHubClient();
            var pubKey = await client.Repository.Actions.Secrets.GetPublicKey(_repoInfo.RepoOwner, _repoInfo.RepoName);

            UpsertRepositorySecret encryptedSecret = new UpsertRepositorySecret();
            if (pubKey != null)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(addEditSecret.SecretValue);
                var key = Convert.FromBase64String(pubKey.Key);
                var sealedKeyBox = Sodium.SealedPublicKeyBox.Create(bytes, key);
                encryptedSecret.KeyId = pubKey.KeyId;
                encryptedSecret.EncryptedValue = Convert.ToBase64String(sealedKeyBox);
                _ = await client.Repository.Actions.Secrets.CreateOrUpdate(_repoInfo.RepoOwner, _repoInfo.RepoName, addEditSecret.SecretName, encryptedSecret);
            }
            await RefreshSecretsAsync(client);
        }
    }

    private void ViewLog_Click(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TextBlock tvi = GetParentTreeViewItem(menuItem);

        // check the tag value to ensure it isn't null
        if (tvi is not null && tvi.Tag is not null)
        {
            string logUrl = tvi.Tag.ToString();
            Process.Start(logUrl);
        }
    }

    private void RunWorkflow_Click(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TextBlock tvi = GetParentTreeViewItem(menuItem);

        // check the tag value to ensure it isn't null
        if (tvi is not null && tvi.Tag is not null)
        {
            GitHubClient client = GetGitHubClient();
            CreateWorkflowDispatch cwd = new CreateWorkflowDispatch(_repoInfo.CurrentBranch);

            try
            {
                _ = client.Actions.Workflows.CreateDispatch(_repoInfo.RepoOwner, _repoInfo.RepoName, (long)tvi.Tag, cwd);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start workflow: {ex.Message}");
            }
        }
    }

    private async void Secret_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // get the items Tag
        if (sender is TreeViewItem item && item.Header is not null && item.Header.ToString().ToLowerInvariant().Contains(" ("))
        {
            string header = item.Header.ToString();
            string secretName = header.Substring(0, header.IndexOf(" ("));

            if (secretName.ToLowerInvariant() != resx.NO_REPO_SECRETS.ToLowerInvariant() && secretName.ToLowerInvariant() != resx.HEADER_REPO_SECRETS.ToLowerInvariant())
            {
                await UpsertRepositorySecret(secretName);
                e.Handled = true;
            }
        }
    }

    private void CancelRun_Click(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TextBlock tvi = GetParentTreeViewItem(menuItem);

        // check the tag value to ensure it isn't null
        if (tvi is not null && tvi.DataContext is not null)
        {
            var run = tvi.DataContext as BaseWorkflowType;
            if (run is not null && run.Id is not null && !ConclusionFilter.IsFinished(run.Conclusion))
            {
                GitHubClient client = GetGitHubClient();
                _ = client.Actions.Workflows.Runs.Cancel(_repoInfo.RepoOwner, _repoInfo.RepoName, Int64.Parse(run.Id));
            }
        }
    }
}

