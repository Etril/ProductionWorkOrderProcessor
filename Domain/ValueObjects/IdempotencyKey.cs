
namespace Domain.ValueObjects; 

public class IdempotencyKey
{
    public Guid Value {get;}

    public IdempotencyKey(Guid? value = null)
    {
        Value= value ?? Guid.NewGuid();
    }

    public override bool Equals (object? obj)
    {
        if (obj is not IdempotencyKey other) return false;
        return Value == other.Value;
    }

    public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

    public override string ToString() => Value.ToString();


}