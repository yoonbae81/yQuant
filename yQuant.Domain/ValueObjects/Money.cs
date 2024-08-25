using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yQuant.Domain.ValueObjects;
public class Money(decimal amount, string currency)
{
    public decimal Amount => amount;
    public string Currency => currency;

    public override string ToString()
    {
        return currency + " " + amount;
    }
}