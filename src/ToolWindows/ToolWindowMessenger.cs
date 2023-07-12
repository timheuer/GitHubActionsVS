using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubActionsVS.ToolWindows;
public class ToolWindowMessenger
{
    public void Send(string message)
    {
        MessageReceived?.Invoke(this, message);
    }
    public event EventHandler<string> MessageReceived;
}
