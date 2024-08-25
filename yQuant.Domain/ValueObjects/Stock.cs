using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yQuant.Domain.ValueObjects;
public class Stock(string symbol)
{
    public string Symbol { init; get; } = symbol;

    public decimal Price { get; }
}