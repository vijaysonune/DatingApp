using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.DTOs
{
    public class CreateMessageDto
    {
        public string RecepientUsername { get; set; }

        public string Content { get; set; }
    }
}
