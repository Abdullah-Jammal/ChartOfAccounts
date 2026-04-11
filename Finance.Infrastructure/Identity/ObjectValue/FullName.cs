namespace Finance.Infrastructure.Identity.ObjectValue;

public class FullName
{
    public string Value { get; private set; } = string.Empty;

    private FullName() { }

    public FullName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Full name cannot be empty");

        if (value.Length < 3)
            throw new ArgumentException("Full name must be at least 3 characters");

        Value = value.Trim();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FullName other)
            return false;

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(FullName fullName)
        => fullName.Value;

    public static explicit operator FullName(string value)
        => new(value);
}
