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

public class ExtensionOptions : BaseOptionModel<ExtensionOptions>
{
    [Category("Query Settings")]
    [DisplayName("Max Runs")]
    [Description("The maximum number of runs to retrieve")]
    [DefaultValue(10)]
    public int MaxRuns { get; set; } = 10;
}
