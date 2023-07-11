using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubActionsVS.Helpers;
internal class RepoInfo
{
    public bool IsGitHub { get; set; }
    public string RepoName { get; set; }
    public string RepoOwner { get; set; }
    public string RepoUrl { get; set; }
    public string CurrentBranch { get; set; }


    internal void FindGitFolder(string path, out string foundPath)
    {
        foundPath = null;
        // Check if the current directory contains a .git folder
        if (Directory.Exists(Path.Combine(path, ".git")))
        {
            foundPath = path;
            var repo = new LibGit2Sharp.Repository(foundPath);
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote is not null)
            {
                var url = remote.Url;
                if (url.Contains("github.com"))
                {
                    IsGitHub = true;
                    var parts = url.Split('/');
                    RepoOwner = parts[parts.Length - 2];
                    RepoName = parts[parts.Length - 1].Replace(".git", "");
                    RepoUrl = url.Replace(".git", "");
                    CurrentBranch = repo.Head.FriendlyName;
                }
            }
            return;
        }
        else
        {
            // Climb up to the parent directory
            string parentPath = Directory.GetParent(path)?.FullName;
            if (!string.IsNullOrEmpty(parentPath))
            {
                FindGitFolder(parentPath, out foundPath); // Recursively search the parent directory
            }
        }

        return;
    }
}
