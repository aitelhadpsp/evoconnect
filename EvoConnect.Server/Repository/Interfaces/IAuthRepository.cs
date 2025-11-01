using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EvoConnect.Server.Data;
using EvoConnect.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;

namespace EvoConnect.Server.Repository.Interfaces;

  
    public interface IAuthRepository
    {
        Task<Utilisateur?> GetUserByIdAsync(int userId);
        Task<AuthResult> AuthenticateAsync(string username, string password);
        int? GetAuthenticatedUserId();
    }


public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public Utilisateur? User { get; set; }
    public string? Message { get; set; }
}
        public interface IJwtTokenGenerator
    {
        string GenerateToken(Utilisateur user);
    }