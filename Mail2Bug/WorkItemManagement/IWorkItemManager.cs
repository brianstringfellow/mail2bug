using System.Collections.Generic;
using Mail2Bug.MessageProcessingStrategies;

namespace Mail2Bug.WorkItemManagement
{
    public interface IWorkItemManager
    {
        void AttachFiles(long workItemId, List<string> fileList);

        SortedList<string, long> WorkItemsCache { get; }

        void CacheWorkItem(long workItemId);

        long CreateWorkItem(Dictionary<string, string> values, Dictionary<string, string> overrides);

        /// <param name="workItemId">The ID of the bug to modify </param>
        /// <param name="comment">Comment to add to description</param>
        /// <param name="values">List of fields to change</param>
        void ModifyWorkItem(long workItemId, string comment, Dictionary<string, string> values);

        INameResolver GetNameResolver();
    }
}