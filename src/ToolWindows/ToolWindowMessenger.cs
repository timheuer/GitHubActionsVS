namespace GitHubActionsVS.ToolWindows;
public class ToolWindowMessenger
{
    public void Send(MessagePayload payload)
    {
        MessageReceived?.Invoke(this, payload);
    }

    public event EventHandler<MessagePayload> MessageReceived;
}
