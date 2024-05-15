namespace TestRequestFileType.ExcelOutputFormatter.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ExcelColumnAttribute : Attribute
{
    public string Name { get; set; }
}
