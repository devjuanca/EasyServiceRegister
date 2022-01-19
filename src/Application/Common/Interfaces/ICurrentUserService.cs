namespace Application.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    string Name { get; }
    Task<string> JwtTokenAsync { get; }
}
