using Native.Sdk.Cqp;
using Native.Sdk.Cqp.Model;

namespace com.moehero.cuckoo.Code.Functions
{
    internal class RemoveAdminFunction : BaseFunction
    {
        private readonly Group _group;
        private readonly QQ _qq;
        private readonly string _adminQQ;

        public RemoveAdminFunction(Group group, QQ qq, string adminQQ) {
            _group = group;
            _qq = qq;
            _adminQQ = adminQQ;
        }

        public override bool CanRun() => _qq == Config.OwnerNumber;

        public override void Run() {
            if(!long.TryParse(_adminQQ, out long qq) || !Config.Admins.Contains(qq)) return;
            Config.Admins.Remove(qq);
            _group.SendGroupMessage($"已取消{CQApi.CQCode_At(qq)}的管理员身份!");
        }
    }
}
