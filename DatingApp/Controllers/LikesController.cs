using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikesController : ControllerBase 
    {
        
 

     
        private readonly IUnitOfWork unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {            
            
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await unitOfWork.userRepository.GetUserByIdAsync(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser == null) return BadRequest("You can not like yourself");

            var userLike = await unitOfWork.likeRepository.GetUserLike(sourceUserId, likedUser.Id);

            if(userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);
          //  _context.Likes.Add(userLike);



            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Falied to like user");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikeParams likeParams)
        {
            likeParams.UserId = User.GetUserId();
            var users = await unitOfWork.likeRepository.GetUserLikes(likeParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
 
    }
}