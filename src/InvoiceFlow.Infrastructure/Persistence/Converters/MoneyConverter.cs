using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InvoiceFlow.Infrastructure.Persistence.Converters;

public sealed class MoneyConverter : ValueConverter<decimal, long>
{
    public MoneyConverter()
        : base(
            amount => checked((long)(amount * 100m)),
            stored => stored / 100m)
    {
    }
}