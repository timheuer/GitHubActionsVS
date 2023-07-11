using CredentialManagement;

namespace GitHubActionsVS.Helpers;
internal class CredentialManager
{
    public static UserPass GetCredentials(string target)
    {
        using var cm = new Credential { Target = target };
        if (!cm.Exists())
        {
            return null;
        }

        cm.Load();
        return new UserPass { Username = cm.Username, Password = cm.Password };
    }

    public class UserPass
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
