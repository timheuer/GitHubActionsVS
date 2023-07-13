namespace GitHubActionsVS.ToolWindows;

public readonly record struct MessagePayload(
    MessageCommand Command,
    string Text = default);
