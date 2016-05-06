using System.Collections.Generic;

namespace Mail2Bug.Email.EWS
{
    public interface IMailFolder
    {
        IEnumerable<IIncomingEmailMessage> GetMessages();
    }
}