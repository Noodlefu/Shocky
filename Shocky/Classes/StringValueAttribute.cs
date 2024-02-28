using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
internal sealed class StringValueAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}
