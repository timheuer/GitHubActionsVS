using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace GitHubActionsVS.Models
{
    sealed class WorkflowRoot
    {
        [YamlMember(Alias = "on")]
        public OnSection On { get; init; }
    }

    sealed class OnSection
    {
        [YamlMember(Alias = "workflow_dispatch")]
        public WorkflowDispatch WorkflowDispatch { get; init; }
    }

    sealed class WorkflowDispatch
    {
        public Dictionary<string, Dictionary<string, object>> Inputs { get; init; }
    }
}
