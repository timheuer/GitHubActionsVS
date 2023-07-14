namespace GitHubActionsVS.Models;

public class SimpleJob : SimpleRun
{
    public override string DisplayName => Name;
}
