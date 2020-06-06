using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TestSingalR.Context;
using TestSingalR.Dto;
using TestSingalR.Entity;
using TestSingalR.Errors;
using TestSingalR.Interfaces;

namespace TestSingalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        public readonly UserManager<AppUser> _userManager;
        public readonly SignInManager<AppUser> _signInManager;
        private readonly DataContext _context;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IUserAccessor _userAccessor;
        private readonly IMapper _mapper;
        public UserController(DataContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            IJwtGenerator jwtGenerator, IUserAccessor userAccessor, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtGenerator = jwtGenerator;
            _userAccessor = userAccessor;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized);
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.Succeeded)
            {
                return Ok(new UserDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Token = _jwtGenerator.CreateToken(user),
                    UserName = user.UserName
                });
            }
            else throw new RestException(HttpStatusCode.Unauthorized);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.Where(x => x.Email == registerDto.Email).AnyAsync())
            {
                throw new RestException(HttpStatusCode.BadRequest, new { Email = "Email already exists" });
            }

            if (await _context.Users.Where(x => x.UserName == registerDto.UserName).AnyAsync())
            {
                throw new RestException(HttpStatusCode.BadRequest, new { UserName = "Username already exists" });
            }

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return Ok(_mapper.Map<AppUser, AppUserDto>(user));
            }
            else throw new Exception("Problem creating user");
        }

        [HttpGet("user")]
        public async Task<IActionResult> CurrentUser()
        {
            var user = await _userManager.FindByNameAsync(_userAccessor.GetCurrentUserName());

            return Ok(_mapper.Map<AppUser, AppUserDto>(user));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _userManager.Users.ToListAsync();

            return Ok(_mapper.Map<List<AppUser>, List<AppUserDto>>(users));
        }
    }
}