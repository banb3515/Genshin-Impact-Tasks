using System.Collections.Generic;

namespace Genshin_Impact_Tasks.Models
{
    public class TaskComparer : IEqualityComparer<TaskModel>
    {
        public bool Equals(TaskModel x, TaskModel y)
        {
            return x.No == y.No && x.Content.ToLower() == y.Content.ToLower() && x.IconPath.ToLower() == y.IconPath.ToLower() && x.Status == y.Status;
        }

        public int GetHashCode(TaskModel obj)
        {
            return obj.GetHashCode();
        }
    }
}
