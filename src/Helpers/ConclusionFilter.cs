using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubActionsVS.Helpers;
public class ConclusionFilter
{
    public static bool IsFinished(string conclusion)
    {
        return conclusion.ToLower() switch
        {
            "pending" or "waiting" or "queued" or "in_progress" or "inprogress" or "requested" => false,
            _ => true,
        };
    }
}
