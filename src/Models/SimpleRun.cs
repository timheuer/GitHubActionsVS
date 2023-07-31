using Humanizer;
using System.Collections.Generic;

namespace GitHubActionsVS.Models;
public class SimpleRun : BaseWorkflowType
{
    public List<SimpleJob> Jobs { get; set; }
    public string RunNumber { get; set; }

    public override string DisplayName => $"{Name} #{RunNumber} ({LogDate.Humanize()})";
}
