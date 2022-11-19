using MediatR;
namespace Application.Models;

public class DomainEventNotification<TDomainEvent> : INotification
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}
