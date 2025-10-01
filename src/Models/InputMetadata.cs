using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GitHubActionsVS.Models
{
    public sealed class InputMetadata
    {
        public string Name { get; }
        public string Type { get; }
        public string Description { get; }
        public bool Required { get; }
        public string Default { get; }
        public string[] Options { get; }

        public InputMetadata(string name, string type, string description, bool required, string @default, string[] options)
        {
            Name = name;
            Type = type;
            Description = description;
            Required = required;
            Default = @default;
            Options = options;
        }

        public static List<InputMetadata> ToInputMeta(IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> inputs)
        {
            var list = new List<InputMetadata>();

            try
            {
                foreach (var kv in inputs)
                {
                    var name = kv.Key;
                    var dict = kv.Value;

                    dict.TryGetValue("type", out var typeObj);
                    dict.TryGetValue("description", out var descObj);
                    dict.TryGetValue("required", out var reqObj);
                    dict.TryGetValue("default", out var defObj);
                    dict.TryGetValue("options", out var optsObj);

                    var type = (typeObj?.ToString()?.Trim().ToLowerInvariant()) switch
                    {
                        "boolean" => "boolean",
                        "choice" => "choice",
                        "environment" => "environment",
                        _ => "string"
                    };

                    string[] options = null;

                    if (optsObj is IEnumerable<object> seq)
                    {
                        options = [.. seq.Select(o => o?.ToString() ?? string.Empty)];
                    }
                    else if (optsObj is string s)
                    {
                        options = [s];
                    }

                    var required = reqObj is bool b ? b : bool.TryParse(reqObj?.ToString(), out var br) && br;

                    list.Add(new InputMetadata(
                        name,
                        type,
                        descObj?.ToString(),
                        required,
                        defObj?.ToString(),
                        options
                    ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create InputMetadata. Ex: {ex.Message}");
            }

            return list;
        }
    }
}
