using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AppUser> userManager;
      
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public AdminController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
            IMapper mapper, IPhotoService photoService )
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;           
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [Authorize (Policy ="RequiredAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                             .Include(r => r.AppUserRoles)
                             .ThenInclude(r=> r.Role)
                             .OrderBy(u=> u.UserName)
                             .Select( u=> new
                             {
                                 u.Id,
                                 Username=u.UserName,
                                 Roles=u.AppUserRoles.Select(r=> r.Role.Name).ToList()
                             })
                             .ToListAsync();

            return Ok(users);

        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username,[FromQuery]string roles)
        {
            var selectedRoles = roles.Split(',').ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not found user");

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add roles");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove roles");

            return Ok(await userManager.GetRolesAsync(user));

        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photo-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("admin and moderator can see this");
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpGet("GetPhotoForApproval")]
        public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotoForApproval()
        {
            var unApprovedPhotos= await unitOfWork.photoRepository.GetUnApprovedPhotosAsync();

            return Ok(unApprovedPhotos);
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpPut("ApprovePhoto")]
        public async Task<ActionResult> ApprovePhoto(PhotoForApprovalDto photoForApprovalDto)
        {
            var user = await unitOfWork.userRepository.GetUserByUsernameAsync(photoForApprovalDto.Username);
            var photo = await unitOfWork.photoRepository.GetPhotoById(photoForApprovalDto.Id);

            if (photo == null) return NotFound("Photo not found");

            photo.IsApproved = true;

            if(user.Photos.All(x=>x.IsMain == false))
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Falied to approve the photo");
        }


        [Authorize(Policy = "RequiredAdminRole")]
        [HttpDelete("reject-photo-byAdmin/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await unitOfWork.photoRepository.GetPhotoById(photoId);          

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You can not delete main photo");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            unitOfWork.photoRepository.RemovePhoto(photo);
            
            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Falied to delete photo");
        }

    }
}