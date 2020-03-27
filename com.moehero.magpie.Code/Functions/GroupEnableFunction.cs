using Native.Sdk.Cqp.Model;

namespace com.moehero.cuckoo.Code.Functions
{
    internal class GroupEnableFunction : BaseFunction
    {
        private readonly Group _group;
        private readonly QQ _qq;

        public GroupEnableFunction(Group group, QQ qq) {
            _group = group;
            _qq = qq;
        }

        public override bool CanRun() => _qq == Config.OwnerNumber;

        public override void Run() {
            if(Config.EnabledGroups.Contains(_group.Id)) return;
            Config.EnabledGroups.Add(_group.Id);
            _group.SendGroupMessage("本群已启用通知!");
        }
    }
}
