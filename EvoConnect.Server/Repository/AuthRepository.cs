using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace EvoConnect.Server.Repository;

public class AuthRepository : IAuthRepository
    {
        private readonly ClinicDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(
            ClinicDbContext context,
            IJwtTokenGenerator jwtTokenGenerator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get user by ID with associated Personne
        /// </summary>
        public async Task<Utilisateur?> GetUserByIdAsync(int userId)
        {
            return await _context.Utilisateurs
                .Include(u => u.Personne)
                .FirstOrDefaultAsync(u => u.IdUtil == userId);
        }

        /// <summary>
        /// Authenticate user with username and password, generate JWT token
        /// </summary>
        public async Task<AuthResult> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Username is required"
                };
            }

            var user = await _context.Utilisateurs
                .Include(u => u.Personne)
                .FirstOrDefaultAsync(u => u.Identifiant == username);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Simple password comparison (not hashed)
            if (user.MotDePasse?.Trim() != password.Trim())
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(user);

            // Update last connection time
            user.DerniereConnexion = DateTime.Now;
            user.Connecte = 1;
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Token = token,
                User = user,
                Message = "Authentication successful"
            };
        }

        /// <summary>
        /// Get authenticated user ID from current HTTP context
        /// </summary>
        public int? GetAuthenticatedUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }
    }


    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Utilisateur user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUtil.ToString()),
                new Claim(ClaimTypes.Name, user.Identifiant ?? string.Empty),
                new Claim("PersonneId", user.IdPersonne?.ToString() ?? string.Empty),
                new Claim("UserType", user.Type?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }




