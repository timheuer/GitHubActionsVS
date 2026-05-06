using System.IO;
using System.Linq;

namespace GitHubActionsVS.Helpers;
internal class RepoInfo
{
    public bool IsGitHub { get; set; }
    public string RepoName { get; set; }
    public string RepoOwner { get; set; }
    public string RepoUrl { get; set; }
    public string CurrentBranch { get; set; }

    internal bool TryFindGitFolder(string path, out string foundPath)
    {
        foundPath = null;
        string currentPath = path;

        while (!string.IsNullOrEmpty(currentPath))
        {
            // Check if the current directory contains a .git folder
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                foundPath = currentPath;
                var repo = new LibGit2Sharp.Repository(foundPath);
                var remote = repo.Network.Remotes.FirstOrDefault();
                if (remote is not null)
                {
                    var remoteUri = GitUri.Parse(remote.Url);
                    RepoOwner = remoteUri.GetRepositoryOwner();
                    RepoName = remoteUri.GetRepositoryName();
                    RepoUrl = $"https://{remoteUri.Host}{(remoteUri.Port is not null ? $":{remoteUri.Port}" : "")}/{RepoOwner}/{RepoName}";
                    CurrentBranch = repo.Head.FriendlyName;
                    IsGitHub = remoteUri.Host == "github.com";
                }

                return true;
            }

            // Move to parent directory
            currentPath = Directory.GetParent(currentPath)?.FullName;
        }

        return false;
    }
}
