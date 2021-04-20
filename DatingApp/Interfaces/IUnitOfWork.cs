using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository userRepository { get; }

        IMessageRepository messageRepository { get; }

        ILikeRepository likeRepository { get; }

        Task<bool> Complete();

        bool HasChanges();
    }
}
