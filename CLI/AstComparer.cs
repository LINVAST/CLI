using LINVAST.Nodes;

namespace LINVAST.Cli
{
    internal static class AstComparer
    {
        public static bool HaveSameStructure(ASTNode expected, ASTNode actual, out string? error)
            => CompareStructure(expected, actual, "$", out error);

        public static bool AreEqual(ASTNode expected, ASTNode actual, out string? error)
        {
            if (expected.Equals(actual)) {
                error = null;
                return true;
            }

            return CompareExact(expected, actual, "$", out error);
        }

        private static bool CompareStructure(ASTNode expected, ASTNode actual, string path, out string? error)
        {
            if (expected.GetType() != actual.GetType()) {
                error = $"{path}: expected {expected.NodeType}, got {actual.NodeType}";
                return false;
            }

            if (expected.Children.Count != actual.Children.Count) {
                error = $"{path}/{expected.NodeType}: expected {expected.Children.Count} children, got {actual.Children.Count}";
                return false;
            }

            for (int i = 0; i < expected.Children.Count; i++) {
                if (!CompareStructure(expected.Children[i], actual.Children[i], $"{path}/{expected.NodeType}[{i}]", out error))
                    return false;
            }

            error = null;
            return true;
        }

        private static bool CompareExact(ASTNode expected, ASTNode actual, string path, out string? error)
        {
            if (expected.GetType() != actual.GetType()) {
                error = $"{path}: expected {expected.NodeType}, got {actual.NodeType}";
                return false;
            }

            if (expected.Children.Count != actual.Children.Count) {
                error = $"{path}/{expected.NodeType}: expected {expected.Children.Count} children, got {actual.Children.Count}";
                return false;
            }

            for (int i = 0; i < expected.Children.Count; i++) {
                ASTNode expectedChild = expected.Children[i];
                ASTNode actualChild = actual.Children[i];
                if (!expectedChild.Equals(actualChild))
                    return CompareExact(expectedChild, actualChild, $"{path}/{expected.NodeType}[{i}]", out error);
            }

            error = $"{path}/{expected.NodeType}: expected '{expected.GetText()}', got '{actual.GetText()}'";
            return false;
        }
    }
}
