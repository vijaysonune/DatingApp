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
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
       
        private readonly UserManager<AppUser> userManager;

        private readonly SignInManager<AppUser> signInManager;

        private readonly ITokenService _tokenService;

        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            
            this.userManager = userManager;
            this.signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.Username)) return BadRequest("User already taken");

            if (registerDto == null)
            {
                return BadRequest("No data found!");
            }

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs= user.KnownAs,
                Gender = user.Gender
            };
        }

        public async Task<bool> UserExist(string Username)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == Username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var Appuser = await userManager.Users
                .Include(p => p.Photos)
                .SingleAsync(x => x.UserName == loginDto.Username.ToLower());

            if (Appuser == null) return Unauthorized("User name doesn't exist");

            var result = await signInManager
                .CheckPasswordSignInAsync(Appuser, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized();

            return new UserDto
            {
                Username = Appuser.UserName,
                Token = await _tokenService.CreateToken(Appuser),
                PhotoUrl = Appuser.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = Appuser.KnownAs,
                Gender= Appuser.Gender
            };

        }
    }
}