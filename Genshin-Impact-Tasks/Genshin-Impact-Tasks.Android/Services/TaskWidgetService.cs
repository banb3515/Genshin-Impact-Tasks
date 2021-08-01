using Genshin_Impact_Tasks.Droid.Widgets;

using Android.App;
using Android.Content;
using Android.Widget;

namespace Genshin_Impact_Tasks.Droid.Services
{
    [Service(Permission = "android.permission.BIND_REMOTEVIEWS", Exported = false)]
    public class TaskWidgetService : RemoteViewsService
    {
        public override IRemoteViewsFactory OnGetViewFactory(Intent intent)
        {
            return new TaskListProvider(ApplicationContext);
        }
    }
}