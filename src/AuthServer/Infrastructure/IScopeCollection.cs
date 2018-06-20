using System.Collections.Generic;

namespace AuthServer.Infrastructure
{
    public interface IScopeCollection
    {
        IEnumerable<string> GetScopes();
    }

    public class ScopeCollection : IScopeCollection
    {
        private readonly List<string> scopes;

        public ScopeCollection(List<string> scopes)
        {
            this.scopes = scopes;
        }

        public IEnumerable<string> GetScopes() => scopes;
    }
}
