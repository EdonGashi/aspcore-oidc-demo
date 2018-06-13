namespace AuthServer
{
    public static class AppConstants
    {
        public static class Scopes
        {
            public const string PersonalData = "personal_data";

            public const string PersonalDataReadonly = "personal_data.read";

            public const string ClientData = "client_data";
        }

        public static class Roles
        {
            public const string Admin = "admin";
            public const string Application = "application";
        }
    }
}
