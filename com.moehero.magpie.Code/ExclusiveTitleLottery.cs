using Native.Sdk.Cqp;
using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moehero.magpie.Code
{
    internal static class ExclusiveTitleLottery
    {
        private const int MAX_ATTEMPTS = 3;

        private static Group _group;
        private static List<QQ> _vaildUsers = new List<QQ>();
        private static List<QQ> _winningUsers = new List<QQ>();
        private static Dictionary<int, TitleInfo> _titleInfos = new Dictionary<int, TitleInfo>();
        private static Dictionary<long, int> _attempts = new Dictionary<long, int>();
        private static int _nextIndex = 1;
        private static object _locker = new object();

        public static bool Run(QQ qq, Group group, string message) {
            if(message.StartsWith("设置头衔")) {
                if(!_winningUsers.Contains(qq)) return false;
                GetLottery(qq, message.Substring(4).Trim());
            } else {
                var info = qq.GetGroupMemberInfo(group);
                if(qq != 562416714 && info.MemberType != QQGroupMemberType.Manage) return false;
                if(message == "准备抽奖") {
                    _group = group;
                    Init();
                    return true;
                } else if(message.StartsWith("抽奖")) {
                    if(!uint.TryParse(message.Substring(2).Trim(), out uint count)) return false;
                    Lottery(qq, count);
                    return true;
                } else if(message.StartsWith("同意")) {
                    if(!int.TryParse(message.Substring(2).Trim(), out int index)) return false;
                    Agree(index);
                    return true;
                } else if(message.StartsWith("拒绝")) {
                    if(!int.TryParse(message.Substring(2).Trim(), out int index)) return false;
                    Refuse(index);
                    return true;
                }
            }
            return false;
        }

        public static void Init() {
            _vaildUsers.Clear();
            var memberInfos = _group.GetGroupMemberList();
            _vaildUsers.AddRange(from i in memberInfos where i.LastSpeakDateTime.AddMonths(1) >= DateTime.Now && i.ExclusiveTitle == "" select i.QQ);
            _group.SendGroupMessage($"抽奖准备完成，共检测到符合资格用户{_vaildUsers.Count}人！");
        }

        public static void Lottery(QQ qq, uint count) {
            _winningUsers.Clear();
            if(_group == null) return;
            if(count > _vaildUsers.Count) {
                _group.SendGroupMessage(CQApi.CQCode_At(qq), Environment.NewLine, $"抽奖人数必须小于等于{_vaildUsers.Count}人，请重新输入！");
                return;
            }
            var random = new Random();
            var vaildUsers = new List<QQ>(_vaildUsers);
            while(_winningUsers.Count < count) {
                var user = vaildUsers[random.Next(vaildUsers.Count - 1)];
                vaildUsers.Remove(user);
                _winningUsers.Add(user);
            }
            var message = new StringBuilder();
            message.AppendLine($"{CQApi.CQCode_Emoji(127881)} 抽奖完成，恭喜以下用户中奖！");
            foreach(var u in _winningUsers) {
                message.AppendLine($"{CQApi.CQCode_Emoji(128313)}{CQApi.CQCode_At(u.Id)} ({u.Id})");
            }
            message.Append("请中奖用户发送“设置头衔 头衔”来设置头衔哦~");
            _group.SendGroupMessage(message.ToString());
        }

        public static void GetLottery(QQ qq, string title) {
            if(_attempts.ContainsKey(qq.Id) && _attempts[qq.Id] >= MAX_ATTEMPTS) return;
            if(title.Length > 6) {
                _group.SendGroupMessage(CQApi.CQCode_At(qq), Environment.NewLine, "头衔长度不能大于6个字符，请重新输入！");
                return;
            }
            int index;
            lock(_locker) {
                _winningUsers.Remove(qq);
                _titleInfos.Add(_nextIndex, new TitleInfo { QQ = qq, Title = title });
                if(_attempts.ContainsKey(qq.Id)) _attempts[qq.Id]++;
                else _attempts.Add(qq.Id, 1);
                index = _nextIndex;
                _nextIndex++;
            }
            var message = new StringBuilder();
            message.AppendLine($"{CQApi.CQCode_Emoji(9888)} {CQApi.CQCode_At(qq)}");
            message.AppendLine($"请求设置头衔为 {title}");
            message.AppendLine($"设置次数：{_attempts[qq.Id]}/{MAX_ATTEMPTS}");
            message.AppendLine($"请管理员进行审核！");
            message.Append($"同意 {index} | 拒绝 {index}");
            _group.SendGroupMessage(message.ToString());
        }

        public static void Agree(int index) {
            if(!_titleInfos.TryGetValue(index, out TitleInfo titleInfo)) return;
            _group.SetGroupMemberForeverExclusiveTitle(titleInfo.QQ, titleInfo.Title);
            _titleInfos.Remove(index);
            _attempts.Remove(titleInfo.QQ.Id);
            _group.SendGroupMessage($"已将 {CQApi.CQCode_At(titleInfo.QQ)} 的头衔设置为 {titleInfo.Title}");
        }

        public static void Refuse(int index) {
            if(!_titleInfos.TryGetValue(index, out TitleInfo titleInfo)) return;
            _titleInfos.Remove(index);
            if(_attempts[titleInfo.QQ.Id] >= MAX_ATTEMPTS) {
                _group.SendGroupMessage(CQApi.CQCode_At(titleInfo.QQ), Environment.NewLine, $" 您要设置的头衔 {titleInfo.Title} 被管理员拒绝！", Environment.NewLine, "设置次数已用完，下次请认真设置！");
            } else {
                _winningUsers.Add(titleInfo.QQ);
                _group.SendGroupMessage(CQApi.CQCode_At(titleInfo.QQ), Environment.NewLine, $" 您要设置的头衔 {titleInfo.Title} 被管理员拒绝！", Environment.NewLine, "请发送“设置头衔 头衔”来重新设置头衔！");
            }
        }

        private struct TitleInfo
        {
            public QQ QQ { get; set; }

            public string Title { get; set; }
        }
    }
}
