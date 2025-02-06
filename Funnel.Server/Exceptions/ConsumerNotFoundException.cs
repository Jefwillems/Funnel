namespace Funnel.Server.Exceptions;

public class ConsumerNotFoundException : ArgumentException
{
    public ConsumerNotFoundException()
    {
    }

    public ConsumerNotFoundException(string? message) : base(message)
    {
    }
}