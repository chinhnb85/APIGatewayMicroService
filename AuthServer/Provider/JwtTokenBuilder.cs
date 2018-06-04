using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace AuthServer.Provider
{
    public class JwtTokenBuilder
    {
        private SecurityKey securityKey = null;
        private string subject = string.Empty;
        private string issuer = string.Empty;
        private string audience = string.Empty;
        private Dictionary<string, string> claims = new Dictionary<string, string>();
        private int expiryInMinutes = 5;

        public JwtTokenBuilder AddSecurityKey(SecurityKey securityKey)
        {
            this.securityKey = securityKey;
            return this;
        }

        public JwtTokenBuilder AddSubject(string subject)
        {
            this.subject = subject;
            return this;
        }

        public JwtTokenBuilder AddIssuer(string issuer)
        {
            this.issuer = issuer;
            return this;
        }

        public JwtTokenBuilder AddAudience(string audience)
        {
            this.audience = audience;
            return this;
        }

        public JwtTokenBuilder AddClaim(string type, string value)
        {
            claims.Add(type, value);
            return this;
        }

        public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
        {
            this.claims.Union(claims);
            return this;
        }

        public JwtTokenBuilder AddExpiry(int expiryInMinutes)
        {
            this.expiryInMinutes = expiryInMinutes;
            return this;
        }

        public JwtToken Build()
        {
            EnsureArguments();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"), ClaimValueTypes.Integer64)
            }
            .Union(this.claims.Select(item => new Claim(item.Key, item.Value)));

            var token = new JwtSecurityToken(
                              issuer: issuer,
                              audience: audience,
                              claims: claims,
                              notBefore: DateTime.Now,
                              expires: DateTime.Now.AddMinutes(expiryInMinutes),
                              signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));

            return new JwtToken(token);
        }

        private void EnsureArguments()
        {
            if (securityKey == null)
                throw new ArgumentNullException("Security Key");

            if (string.IsNullOrEmpty(subject))
                throw new ArgumentNullException("Subject");

            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException("Issuer");

            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException("Audience");
        }
    }

    public sealed class JwtToken
    {
        private JwtSecurityToken token;

        internal JwtToken(JwtSecurityToken token)
        {
            this.token = token;
        }

        public string access_token => new JwtSecurityTokenHandler().WriteToken(token);
        public long expires_date => ConvertToTimestamp(token.ValidTo.ToLocalTime());

        //public DateTime created_date => token.ValidFrom.ToLocalTime();        
        //public DateTime expires_date => token.ValidTo.ToLocalTime();
        //public long expires_in => ConvertToTimestamp(token.ValidTo.ToLocalTime());
        //public string token_type => "bearer";

        private static long ConvertToTimestamp(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }
    }
}
