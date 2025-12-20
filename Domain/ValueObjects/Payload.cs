

namespace Domain.ValueObjects;

public class Payload
{

    private Payload ()
    {
        
    }
    public Guid ProductId {get; }

    public decimal Quantity {get;}

    public Payload (Guid productId, decimal quantity)
    {
        if (productId == Guid.Empty) 
        throw new ArgumentException("ProductId cannot be empty"); 


        if (quantity <= 0)
        throw new ArgumentException("Quantity cannot be 0 or negative");

        ProductId = productId;
        Quantity = quantity;
    }
    public override bool Equals (object? obj)
        {
            if (obj is not Payload other) return false;
            return ProductId == other.ProductId && Quantity == other.Quantity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProductId, Quantity);
        }

}