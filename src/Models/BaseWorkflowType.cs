namespace GitHubActionsVS.Models;

public abstract class BaseWorkflowType
{
    public string Name { get; set; }

    public abstract string DisplayName { get; }
    public string Conclusion { get; set; }
    public DateTimeOffset? LogDate { get; set; }
    public string DisplayDate => $"{LogDate:g}";
    public string? Url { get; set; }
    public string Id { get; set; }
}
