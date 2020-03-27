using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;

namespace com.moehero.cuckoo.Code
{
    public class Events :
        IAppEnable,
        IGroupMessage
    {
        public void AppEnable(object sender, CQAppEnableEventArgs e) {
            Config.AppDirectory = e.CQApi.AppDirectory;
        }

        public void GroupMessage(object sender, CQGroupMessageEventArgs e) {
            Routes.Execute(e);
        }
    }
}
