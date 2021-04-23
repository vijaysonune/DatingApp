using AutoMapper;
using DatingApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext dataContext;
        private readonly IMapper mapper;       

        public UnitOfWork(DataContext dataContext, IMapper mapper )
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
            
        }

        public IUserRepository userRepository => new UserRepository(dataContext, mapper);

        public IMessageRepository messageRepository => new MessageRepository(dataContext, mapper);

        public ILikeRepository likeRepository => new LikeRepository(dataContext);

        public IPhotoRepository photoRepository => new PhotoRepository(dataContext, mapper);

        public async Task<bool> Complete()
        {
            return await dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return dataContext.ChangeTracker.HasChanges();
        }
    }
}
