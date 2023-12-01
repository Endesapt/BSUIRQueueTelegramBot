using BSUIRQueueTelegramBot.Data;

namespace BSUIRQueueTelegramBot.Repository
{
    public interface IRepository 
    {
        IEnumerable<Record> GetRecordList(Subject subject);
        
        Record? getInLine(string userName,Subject subject, int position);
        Record? getOutOfLine(string userName, Subject subject);

        void ClearQueue();
    }
}
