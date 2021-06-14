﻿using Event_Creator.models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.Other.Interfaces
{
    public interface IJwtService
    {
        Task<string> JwtTokenGenerator(long userId , string jti);
        Task<RefreshToken> GenerateRefreshToken(string jwtId, long userId , HttpContext httpContext,bool refresh, int priorityNum);
        Task<AuthResponse> RefreshToken(RefreshRequest refreshRequest , HttpContext httpContext);

    }
}
