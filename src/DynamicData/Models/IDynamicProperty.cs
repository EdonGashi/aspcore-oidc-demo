namespace DynamicData.Models
{
    internal interface IDynamicProperty
    {
        string Key { get; set; }

        string Value { get; set; }

        string Flags { get; set; }
    }
}