using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DatingApp.Data;
using DatingApp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using DatingApp.DTOs;
using Microsoft.EntityFrameworkCore;
using DatingApp.Interfaces;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.Username)) return BadRequest("User already taken");
            
            if(registerDto == null)
            {
                return BadRequest("No data found!");
            }
             
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key

            };
             _context.Users.Add(user);
             await _context.SaveChangesAsync();

            return new UserDto
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }

        public async Task<bool> UserExist(string Username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == Username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var Appuser = await _context.Users
                .Include(p=> p.Photos)
                .SingleAsync(x => x.UserName == loginDto.Username);

            if (Appuser == null) return Unauthorized("User name doesn't exist");

            using var hmac= new HMACSHA512(Appuser.PasswordSalt);

            byte[] passwordHashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0;i< passwordHashValue.Length;i++)
            {
                if (Appuser.PasswordHash[i] != passwordHashValue[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                Username = Appuser.UserName,
                Token = _tokenService.CreateToken(Appuser),
                PhotoUrl = Appuser.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };

        }
    }
}