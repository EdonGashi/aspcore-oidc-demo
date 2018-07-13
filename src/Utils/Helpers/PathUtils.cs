using System.Linq;

namespace Utils.Helpers
{
    public static class PathUtils
    {
        public static string Join(params string[] paths)
        {
            if (paths == null)
            {
                return string.Empty;
            }

            paths = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            if (paths.Length == 0)
            {
                return string.Empty;
            }

            var result = paths[0].TrimEnd('/');
            for (var i = 1; i < paths.Length; i++)
            {
                result += "/" + paths[i].TrimStart('/');
            }

            return result;
        }
    }
}
