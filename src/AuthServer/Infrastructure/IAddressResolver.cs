namespace AuthServer.Infrastructure
{
    public interface IAddressResolver
    {
        string Resolve();
    }

    public class AddressResolver : IAddressResolver
    {
        private readonly string address;

        public AddressResolver(string address)
        {
            this.address = address;
        }

        public string Resolve() => address;
    }
}
