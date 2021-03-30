using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
    public interface IPhotoService
    {
       public Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

       public Task<DeletionResult> DeletePhotoAsync(string publicId);

    }
}
