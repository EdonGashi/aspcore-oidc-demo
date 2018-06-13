namespace AuthServer
{
    public static class AppConstants
    {
        public static class Scopes
        {
            public const string ApplicationRead = "app.read";

            public const string ApplicationWrite = "app.write";
        }

        public static class Roles
        {
            public const string Administrator = "administrator";

            public const string Application = "application";
        }

        public static string AdminEmail = "admin@authserver";
    }
}
