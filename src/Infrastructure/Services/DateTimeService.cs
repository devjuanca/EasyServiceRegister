using Application.Interfaces;
using EasyServiceRegister.Attributes;

namespace Tech.CleanArchitecture.Infrastructure.Persistence.Services;

[RegisterAsTransient]
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
