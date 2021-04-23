using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);

        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

        Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);

        Task<string> GetUserGenderAsync(string username);

        Task<AppUser> GetUserByPhotoId(int photoId);
    }
}
