using yQuant.Domain.ValueObjects;

namespace yQuant.Application.Ports.Output;
public interface IAccountPort
{
    Task<Money> GetBalanceAsync();
    Task<Money> GetAvailableBalanceAsync();
}
