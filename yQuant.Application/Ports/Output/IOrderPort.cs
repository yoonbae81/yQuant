namespace yQuant.Application.Ports.Output;
public interface IOrderPort
{
    public Task PlaceOrder(OrderCommand command);
    public decimal CalcEntryVolume(OrderCommand command);
}
