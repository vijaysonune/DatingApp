using DatingApp.DTOs;
using DatingApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnApprovedPhotosAsync();

        Task<Photo> GetPhotoById(int id);

        void RemovePhoto(Photo photo);



    }
}
