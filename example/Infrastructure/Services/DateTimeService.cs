using Application.Interfaces;
using EasyServiceRegister.Attributes;

namespace Infrastructure.Persistence.Services;

[RegisterAsTransient]
public class DateTimeService
{
    public DateTime Now => DateTime.UtcNow;
}
