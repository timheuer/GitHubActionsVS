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
    public string TriggerEvent { get; set; }
    public string TriggerLogin { get; set; }

    public bool HasActions
    {
        get
        {
            return TriggerEvent is not null || Url is not null; 
        }
    }

    public bool Cancellable
    {
        get
        {
            return TriggerEvent is not null && !Helpers.ConclusionFilter.IsFinished(Conclusion);
        }
    }
}
