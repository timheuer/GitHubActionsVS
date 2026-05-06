using System.Text.RegularExpressions;

namespace GitHubActionsVS.Helpers;

internal class GitUri
{
    public string OriginalUri { get; }
    public string Scheme { get; }
    public string Host { get; }
    public int? Port { get; }
    public string Path { get; }

    private GitUri(string originalUri, string scheme, string host, int? port, string path)
    {
        OriginalUri = originalUri;
        Scheme = scheme;
        Host = host;
        Port = port;
        Path = path;
    }

    public static GitUri Parse(string originalUri)
    {
        GitUri gitUri = null;
        originalUri = originalUri?.Trim();
        if (!string.IsNullOrEmpty(originalUri))
        {
            Uri uri;
            if (Uri.TryCreate(originalUri, UriKind.Absolute, out uri) && !string.IsNullOrEmpty(uri.Host))
            {
                gitUri = new GitUri(originalUri, uri.Scheme, uri.Host, GetUriPort(uri), uri.AbsolutePath);
            }
            else if (!originalUri.Contains("://") && Uri.TryCreate("https://" + originalUri, UriKind.Absolute, out uri) && !string.IsNullOrEmpty(uri.Host))
            {
                gitUri = new GitUri(originalUri, null, uri.Host, GetUriPort(uri), uri.AbsolutePath);
            }
            else
            {
                // ssh style git URI: git@github.com:owner/repo.git
                Match match = new Regex("^(?:(?:.+)@)?(?'host'[^\\:\\/]+)\\:(?:(?'port'\\d+/)?)?(?'path'.+)$").Match(originalUri);
                if (match.Success)
                {
                    string host = match.Groups["host"].Value;
                    int? port = null;
                    string portValue = match.Groups["port"].Value;
                    if (!string.IsNullOrEmpty(portValue))
                    {
                        if (int.TryParse(portValue.TrimEnd(['/']), out int parsedPort))
                        {
                            port = new int?(parsedPort);
                        }
                    }
                    string path = "/" + match.Groups["path"].Value;
                    gitUri = new GitUri(originalUri, null, host, port, path);
                }
            }
        }
        if (gitUri == null)
        {
            throw new UriFormatException("The provided URI is not a valid HTTP or SSH URI.");
        }
        return gitUri;
    }

    private static int? GetUriPort(Uri uri)
    {
        if (uri.Port != -1 && !uri.IsDefaultPort)
        {
            return uri.Port;
        }
        return null;
    }

    public string GetRepositoryName()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }
        string[] segments = Path.Split('/');
        if (segments.Length > 0)
        {
            string lastSegment = segments[segments.Length - 1];
            if (lastSegment.EndsWith(".git"))
            {
                return lastSegment.Substring(0, lastSegment.Length - 4);
            }
            return lastSegment;
        }
        return null;
    }

    public string GetRepositoryOwner()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }
        string[] segments = Path.Split('/');
        if (segments.Length > 1)
        {
            return segments[segments.Length - 2];
        }
        return null;
    }
}
