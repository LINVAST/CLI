using System.Collections.Generic;
using System.Linq;
using System.Text;
using LINVAST.Nodes;

namespace LINVAST.Cli
{
    internal static class AstStatistics
    {
        public static string Describe(ASTNode root)
        {
            Dictionary<string, int> counts = new();
            int total = 0;
            int maxDepth = 0;

            Visit(root, 0);

            var sb = new StringBuilder();
            sb.AppendLine("AST statistics");
            sb.AppendLine($"  Total nodes: {total}");
            sb.AppendLine($"  Max depth:   {maxDepth}");
            sb.AppendLine("  Node types:");
            foreach ((string type, int count) in counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
                sb.AppendLine($"    {type}: {count}");

            return sb.ToString().TrimEnd();

            void Visit(ASTNode node, int depth)
            {
                total++;
                maxDepth = System.Math.Max(maxDepth, depth);

                string type = node.NodeType;
                counts[type] = counts.TryGetValue(type, out int existing) ? existing + 1 : 1;

                foreach (ASTNode child in node.Children)
                    Visit(child, depth + 1);
            }
        }
    }
}
