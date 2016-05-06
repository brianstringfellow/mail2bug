using System.Collections.Generic;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;

namespace Mail2Bug.Email.EWS
{
    using Microsoft.AzureAd.Icm.Utility;
    using log4net;

    class EWSMailFolder : IMailFolder
    {
        private readonly Folder _folder;
        private readonly PullSubscription subscription;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EWSMailFolder));

        public EWSMailFolder(Folder folder)
        {
            _folder = folder;
            subscription = _folder.Service.SubscribeToPullNotifications(new FolderId[] { folder.Id }, 30, null, EventType.NewMail);
        }

        public IEnumerable<IIncomingEmailMessage> GetMessages()
        {
            Logger.InfoFormat("Getting email messages...");
            var itemCount = _folder.TotalCount;
            if (itemCount <= 0)
            {
                return new List<IIncomingEmailMessage>();
            }

            List<Item> items = new List<Item>();

            // Gets new email messages that were recieved after the initial startup.
            GetEventsResults eventResults = subscription.GetEvents();
            foreach (ItemEvent itemEvent in eventResults.ItemEvents)
            {
                Item item = Item.Bind(_folder.Service, itemEvent.ItemId);
                items.Add(item);
            }

            // Gets new email messages on the initial startup.
            var view = new ItemView(itemCount);
            List<Item> itemsToAdd = _folder.FindItems(view).ToList();
            items.AddRange(itemsToAdd);

            Logger.InfoFormat("Items found: {0}", items.Count());

            var messages = new List<IIncomingEmailMessage>();
            int junkCount = 0;
            foreach (var item in items)
            {
                if (item is EmailMessage)
                {
                    EWSIncomingMessage message = new EWSIncomingMessage((EmailMessage)item);
                    messages.Add(message);
                }
                else
                {
                    junkCount++;
                    item.Move(WellKnownFolderName.DeletedItems);
                }    
            }

            Logger.InfoFormat("Message count: {0}, Junk count: {1}", messages.Count, junkCount);
            Logger.InfoFormat("Completed getting messages.");
            return messages;
        }
    }
}
