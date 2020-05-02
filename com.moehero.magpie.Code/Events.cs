using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;

namespace com.moehero.magpie.Code
{
    public class Events :
        IAppEnable,
        IGroupMessage
    {
        public static CQApi CQApi;

        public void AppEnable(object sender, CQAppEnableEventArgs e) {
            CQApi = e.CQApi;
        }

        public void GroupMessage(object sender, CQGroupMessageEventArgs e) {
            if(e.FromGroup != 1053351249 || e.FromGroup != 303255550) return;
            e.Handler = ExclusiveTitleLottery.Run(e.FromQQ, e.FromGroup, e.Message.Text);
        }
    }
}
