using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public string SenderUsername { get; set; }

        public int SenderId { get; set; }

        public AppUser Sender { get; set; }

        public string RecepientUsername { get; set; }

        public int RecepientId { get; set; }

        public AppUser Recepient { get; set; }

        public string Content { get; set; }

        public DateTime MessageSent { get; set; } = DateTime.Now;

        public DateTime? DateRead { get; set; }

        public bool SenderDeleted { get; set; }

        public bool RecepientDeleted { get; set; }


    }
}
