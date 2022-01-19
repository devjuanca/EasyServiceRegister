using Application.Interfaces;
using ServiceInyector.Interfaces;

namespace Tech.CleanArchitecture.Infrastructure.Persistence.Services;

public class DateTimeService : IDateTime, IRegisterAsTranscient
{
    public DateTime Now => DateTime.UtcNow;
}
