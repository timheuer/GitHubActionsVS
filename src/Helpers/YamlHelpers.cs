using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace GitHubActionsVS.Helpers
{
    internal static class YamlHelpers
    {
        /// <summary>
        /// Follows YamlDotNet alias nodes (&anchor / *alias) to the real node.
        /// Returns the resolved node if an alias chain exists.
        /// </summary>
        public static YamlNode Unalias(YamlNode node)
        {
            // Use reflection to check for YamlAliasNode and access RealNode
            var aliasType = node.GetType().FullName == "YamlDotNet.RepresentationModel.YamlAliasNode"
                ? node.GetType()
                : null;

            while (aliasType != null)
            {
                var realNodeProp = aliasType.GetProperty("RealNode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (realNodeProp?.GetValue(node) is not YamlNode realNode)
                    break;
                node = realNode;
                aliasType = node.GetType().FullName == "YamlDotNet.RepresentationModel.YamlAliasNode"
                    ? node.GetType()
                    : null;
            }

            return node;
        }

        /// <summary>
        /// Attempts to retrieve a child value from a mapping node by key name.
        /// Uses string comparison on scalar node values rather than allocating new nodes.
        /// </summary>
        /// <param name="map">The mapping node to search.</param>
        /// <param name="key">The scalar key string to match.</param>
        /// <param name="value">The value node if found.</param>
        /// <returns>True if a matching key was found, otherwise false.</returns>
        public static bool TryGetScalarKey(YamlMappingNode map, string key, out YamlNode value)
        {
            foreach (var kv in map.Children)
            {
                if (kv.Key is YamlScalarNode sk &&
                    string.Equals(sk.Value, key, StringComparison.Ordinal))
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = null!;
            return false;
        }

    }
}
