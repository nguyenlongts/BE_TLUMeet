using AuthService.Domain.Interfaces;
using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
           var result = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token.ToString() == token);
           return result;
        }

        public async Task<bool> RevokeAllByUserIdAsync(int userId)
        {
            var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId && t.RevokeAt == null).ToListAsync();
            foreach (var token in tokens)
            {
                token.RevokeAt = DateTime.UtcNow;
            }

            return true;
        }

        public async Task<bool> RevokeAsync(string token)
        {
            var result = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token.ToString() == token);
            if (result == null)
            {
                return false;
            }
            result.RevokeAt = DateTime.UtcNow;


            return true;
        }

        public async Task SaveAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);


        }
    }
}
