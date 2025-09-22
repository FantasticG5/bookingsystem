namespace Infrastructure.Interfaces;

// Interface för att hantera events
public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent eventData);
}