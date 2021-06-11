using Event_Creator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Interfaces
{
    public interface IJwtService
    {
        Task<string> JwtTokenGenerator(long userId , string jti);
        Task<RefreshToken> GenerateRefreshToken(string jwtId, long userId , string ip);
        Task<AuthResponse> RefreshToken(RefreshRequest refreshRequest , string ip);

    }
}
