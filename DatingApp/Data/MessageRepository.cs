using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper mapper;

        public MessageRepository( DataContext context , IMapper mapper )
        {
            _context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                    .Include(x => x.Connections)
                    .Where(x => x.Connections.Any(y => y.ConnectionId == connectionId))
                    .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(u=> u.Sender)
                .Include(u=> u.Recepient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecepientUsername == messageParams.Username && u.RecepientDeleted==false),

                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted== false),

                _ => query.Where(u => u.RecepientUsername == messageParams.Username
                && u.RecepientDeleted==false  && u.DateRead==null)
            };
                    

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                    .Include(x => x.Connections)
                    .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recepientUsername)
        {
            var messages = await _context.Messages               
                .Where(m => m.Sender.UserName == currentUsername && m.RecepientDeleted == false
                && m.Recepient.UserName == recepientUsername
                || m.Recepient.UserName == currentUsername && m.SenderDeleted == false &&
                m.Sender.UserName == recepientUsername)
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = _context.Messages.Where(m => m.DateRead == null &&
            m.RecepientUsername == currentUsername).ToList();

            if(unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }                
            }

            return messages;

        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
       
    }
}
