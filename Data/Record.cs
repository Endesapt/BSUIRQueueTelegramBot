using Microsoft.EntityFrameworkCore;

namespace BSUIRQueueTelegramBot.Data
{
    [Keyless]
    public class Record
    {
        public string UserName { get; set; }
        public Subject Subject { get; set; }
        public int Place { get;set; }
    }
}
