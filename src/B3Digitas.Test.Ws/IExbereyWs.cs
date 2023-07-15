namespace B3Digitas.Test.Ws;

public interface IExbereyWs
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
}