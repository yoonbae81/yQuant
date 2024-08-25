using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yQuant.Domain.Entities;
public class Account
{
    private readonly Money baselineBalance;

    public Account(Money baselineBalance)
    {
        this.baselineBalance = baselineBalance;
    }
}