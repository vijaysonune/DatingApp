using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatingApp.Extensions;
using DatingApp.Helpers;

namespace DatingApp.Controllers
{
 
    public class UsersController : BaseApiController
    {
        
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork unitOfWork;

        public UsersController(IMapper mapper, IPhotoService photoService, IUnitOfWork unitOfWork)
        {
            
            _mapper = mapper;
            _photoService = photoService;
            this.unitOfWork = unitOfWork;
        }

        //localhost:49716/api/Users
        
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await unitOfWork.userRepository.GetUserGenderAsync(User.GetUserName());
            userParams.CurrentUsername = User.GetUserName();

            if ( string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender= gender == "male" ? "female" : "male";
            }

            var users = await unitOfWork.userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            return Ok(users);

             
        }

        //localhost:49716/api/Users/username
        [Authorize(Roles = "Member")]
        [HttpGet("{username}" ,Name = "GetUser") ]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await unitOfWork.userRepository.GetMemberAsync(username);           
           
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdate)
        {            
            var user = await unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUserName());

            _mapper.Map(memberUpdate, user);

            unitOfWork.userRepository.Update(user);

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUserName());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest();

            var photo = new Photo
            {
                Url = result.Url.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count==0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);
            if (await unitOfWork.Complete())
            {
                // return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            

            return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This photo is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.userRepository.GetUserByUsernameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You can not delete main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Falied to delete photo");
        }


    }
}