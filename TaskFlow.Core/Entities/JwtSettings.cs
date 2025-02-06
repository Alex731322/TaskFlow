﻿namespace TaskFlow.Core.Entities
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpireMinutes { get; set; }
        public int RefreshTokenExpireDays { get; set; }
    }

}
