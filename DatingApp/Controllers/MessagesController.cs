using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
     
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
          
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {
            var username = User.GetUserName();
            
            if(username == createMessage.RecepientUsername.ToLower())            
                BadRequest("You can not send messages to yourself");

            var sender = await unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var recepient = await unitOfWork.userRepository.GetUserByUsernameAsync(createMessage.RecepientUsername);

            if (recepient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recepient = recepient,
                SenderUsername = sender.UserName,
                RecepientUsername = recepient.UserName,
                Content = createMessage.Content
            };

            unitOfWork.messageRepository.AddMessage(message);

            if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<IEnumerable<MessageDto>> GetMessagesForUser([FromQuery]
        MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var messages = await unitOfWork.messageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, 
                messages.TotalCount, messages.TotalPages);

            return messages;
        }

      
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await unitOfWork.messageRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recepient.UserName != username)
                return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recepient.UserName == username) message.RecepientDeleted = true;

            if (message.SenderDeleted == true && message.RecepientDeleted == true)
                unitOfWork.messageRepository.DeleteMessage(message);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Problem in deleting a message");

        }
    }
}