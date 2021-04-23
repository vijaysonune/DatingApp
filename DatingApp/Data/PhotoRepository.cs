using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext dataContext;

        private readonly IMapper mapper;

        public PhotoRepository(DataContext dataContext, IMapper mapper )
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
        }
        public async Task<Photo> GetPhotoById(int id)
        {
            return await dataContext.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(x=> x.Id==id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotosAsync()
        {

            return await dataContext.Photos
                             .IgnoreQueryFilters()
                             .Where(x => x.IsApproved == false)
                              .Select(u => new PhotoForApprovalDto
                              {
                                  Id = u.Id,
                                  Username = u.AppUser.UserName,
                                  Url = u.Url,
                                  IsApproved = u.IsApproved
                              }).ToListAsync();

          
            
         }

        public void RemovePhoto(Photo photo)
        {
            dataContext.Photos.Remove(photo);
        }
    }
}
