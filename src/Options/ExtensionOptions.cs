using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GitHubActionsVS;
internal partial class OptionsProvider
{
    // Register the options with this attribute on your package class:
    // [ProvideOptionPage(typeof(OptionsProvider.ExtensionOptionsOptions), "GitHubActionsVS", "ExtensionOptions", 0, 0, true, SupportsProfiles = true)]
    [ComVisible(true)]
    public class ExtensionOptionsOptions : BaseOptionPage<ExtensionOptions> { }
}

public class ExtensionOptions : BaseOptionModel<ExtensionOptions>, IRatingConfig
{
    [Category("Query Settings")]
    [DisplayName("Max Runs")]
    [Description("The maximum number of runs to retrieve")]
    [DefaultValue(10)]
    public int MaxRuns { get; set; } = 10;

    [Category("Query Settings")]
    [DisplayName("Refresh Active Jobs")]
    [Description("Whether to poll/refresh when pending/active jobs are going")]
    [DefaultValue(false)]
    public bool RefreshActiveJobs { get; set; } = false;

    [Category("Query Settings")]
    [DisplayName("Refresh Interval (in seconds)")]
    [Description("The interval (in seconds) to poll/refresh when pending/active jobs are going")]
    [DefaultValue(5)]
    public int RefreshInterval { get; set; } = 5;

    [Browsable(false)]
    public int RatingRequests { get; set; }

    [Browsable(false)]
    [DefaultValue(false)]
    public bool CurrentBranchExpanded { get; set; } = false;

    [Browsable(false)]
    [DefaultValue(true)]
    public bool WorkflowsExpanded { get; set; } = true;

    [Browsable(false)]
    [DefaultValue(false)]
    public bool SecretsExpanded { get; set; } = false;
}
