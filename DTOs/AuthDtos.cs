using System.Collections.Generic;

namespace NexFit.Backend.DTOs
{
    public record RegisterDto(string Email, string Password, string FullName, string Role);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(bool IsSuccess, string Token, string Message);
}