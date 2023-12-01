using BSUIRQueueTelegramBot.Data;
using Microsoft.EntityFrameworkCore;

namespace BSUIRQueueTelegramBot.Repository
{
    public class RecordRepository : IRepository
    {
        private readonly ApplicationContext db;

        public RecordRepository(ApplicationContext db)
        {
            this.db = db;
        }

        public Record? getInLine(string userName, Subject subject, int position)
        {

            Record? userRecord = db.records.Where((r) => r.Subject == subject)
                .FirstOrDefault(r => r.UserName == userName);
            Record? reserved = db.records.Where((r) => r.Subject == subject)
                .FirstOrDefault(r => r.Place == position);
            if (userRecord == null && reserved == null)
            {
                userRecord = new Record()
                {
                    UserName = userName,
                    Subject = subject,
                    Place = position
                };
                db.records.Add(userRecord);
                try
                {
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    return null;
                }
                return userRecord;
            }
            return null;
        }

        public Record getOutOfLine(string userName, Subject subject)
        {
            Record? userRecord = db.records.Where((r) => r.Subject == subject)
                .FirstOrDefault(r => r.UserName == userName);
            if (userRecord != null) {
                db.records.Remove(userRecord);
                db.SaveChanges();
            }
            return userRecord;
            
        }

        public IEnumerable<Record> GetRecordList(Subject subject)
        {
            return db.records.Where((r)=>r.Subject == subject).OrderBy((r)=>r.Place);
        }
        public void ClearQueue()
        {
            db.Database.ExecuteSqlRaw("DELETE FROM records");
        }
    }
}
