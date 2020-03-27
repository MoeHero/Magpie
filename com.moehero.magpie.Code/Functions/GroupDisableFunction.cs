using Native.Sdk.Cqp.Model;

namespace com.moehero.cuckoo.Code.Functions
{
    internal class GroupDisableFunction : BaseFunction
    {
        private readonly Group _group;
        private readonly QQ _qq;

        public GroupDisableFunction(Group group, QQ qq) {
            _group = group;
            _qq = qq;
        }

        public override bool CanRun() => _qq == Config.OwnerNumber;

        public override void Run() {
            if(!Config.EnabledGroups.Contains(_group.Id)) return;
            Config.EnabledGroups.Remove(_group.Id);
            _group.SendGroupMessage("本群已停用通知!");
        }
    }
}
