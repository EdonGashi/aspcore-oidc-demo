using System;

namespace Utils.Initialization
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class StartupOrderAttribute : Attribute
    {
        public StartupOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
