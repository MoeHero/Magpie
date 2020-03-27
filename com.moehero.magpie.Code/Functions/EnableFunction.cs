using Native.Sdk.Cqp.Model;

namespace com.moehero.cuckoo.Code.Functions
{
    internal class EnableFunction : BaseFunction
    {
        private readonly Group _group;
        private readonly QQ _qq;

        public EnableFunction(Group group, QQ qq) {
            _group = group;
            _qq = qq;
        }

        public override bool CanRun() => _qq == Config.OwnerNumber;

        public override void Run() {
            if(Config.Enabled) return;
            Config.Enabled = true;
            _group.SendGroupMessage("机器人已启用!");
        }
    }
}
