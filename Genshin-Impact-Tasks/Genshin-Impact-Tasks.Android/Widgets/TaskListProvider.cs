using Genshin_Impact_Tasks.Models;

using System.Collections.Generic;

using Android.Content;
using Android.Widget;
using Android.OS;

namespace Genshin_Impact_Tasks.Droid.Widgets
{
    public class TaskListProvider : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
    {
        private Context context;

        public TaskListProvider(Context context)
        {
            this.context = context;

            PopulateListItem();
        }

        private void PopulateListItem()
        {
            TaskWidget.Items = new List<TaskModel>();

            switch (TaskWidget.CurrentTask)
            {
                case 0:
                    TaskWidget.Database.Table<DailyTaskTable>().ToList().ForEach(task =>
                    { TaskWidget.Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
                case 1:
                    TaskWidget.Database.Table<WeeklyTaskTable>().ToList().ForEach(task =>
                    { TaskWidget.Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
                case 2:
                    TaskWidget.Database.Table<OneTimeTaskTable>().ToList().ForEach(task =>
                    { if (!task.Status) TaskWidget.Items.Add(new TaskModel { No = task.No, Content = task.Content, IconPath = task.IconPath, Status = task.Status }); });
                    break;
            }
        }

        public int Count { get { return TaskWidget.Items.Count; } }

        public long GetItemId(int position)
        {
            return position;
        }

        public RemoteViews GetViewAt(int position)
        {
            RemoteViews remoteViews = new RemoteViews(context.PackageName, Resource.Layout.task_item);
            var item = TaskWidget.Items[position];
            var imageName = item.IconPath.Split('/')[1].Split('.')[0];
            var background = Resource.Drawable.task_item_background_false;
            if (item.Status) background = Resource.Drawable.task_item_background_true;

            var extras = new Bundle();
            extras.PutInt("ItemNo", item.No);

            var fiIntent = new Intent();
            fiIntent.PutExtras(extras);

            remoteViews.SetOnClickFillInIntent(Resource.Id.task_layout, fiIntent);
            remoteViews.SetInt(Resource.Id.task_layout, "setBackgroundResource", background);
            remoteViews.SetImageViewResource(Resource.Id.task_icon, context.Resources.GetIdentifier(imageName, "drawable", context.PackageName));
            remoteViews.SetTextViewText(Resource.Id.task_content, item.Content);

            return remoteViews;
        }

        public RemoteViews LoadingView { get { return null; } }

        public int ViewTypeCount { get { return 1; } }

        public bool HasStableIds { get { return true; } }


        public void OnCreate() { }

        public void OnDataSetChanged() 
        {
            PopulateListItem();
        }

        public void OnDestroy() { }
    }
}