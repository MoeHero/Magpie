using com.moehero.cuckoo.Code.Functions;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Group = Native.Sdk.Cqp.Model.Group;

namespace com.moehero.cuckoo.Code
{
    internal class Routes
    {
        public static readonly List<Route> GroupRouteList = new List<Route> {
            new Route { URI = "启用机器人", Function = typeof(EnableFunction) },
            new Route { URI = "停用机器人", Function = typeof(DisableFunction) },
            new Route { URI = "本群启用通知", Function = typeof(GroupEnableFunction) },
            new Route { URI = "本群停用通知", Function = typeof(GroupDisableFunction) },
            new Route { URI = "添加管理员{AdminQQ}", Function = typeof(AddAdminFunction) },
            new Route { URI = "移除管理员{AdminQQ}", Function = typeof(RemoveAdminFunction) },
        };

        private static readonly List<string> IgnoreDisable = new List<string> {
            "启用机器人",
            "本群启用通知",
        };

        public static void Execute(CQGroupMessageEventArgs e) {
            if(!Config.Enabled || !Config.EnabledGroups.Contains(e.FromGroup.Id)) {
                if(!IgnoreDisable.Contains(e.Message.Text)) return;
            }
            foreach(var route in GroupRouteList) {
                if(!route.IsMatch(e.Message.Text)) continue;
                var function = route.GetFunctionInstance(e);
                if(function != null && function.CanRun()) function.Run();
                if(function?.Handled == true) return;
            }
        }
    }

    internal class Route
    {
        private static readonly char[] SEPARATOR = new[] { ' ', ',' };
        private Type functions;

        public string URI { get; set; }

        public Type Function {
            get => functions;
            set {
                if(value.IsInstanceOfType(typeof(IFunction))) throw new InvalidOperationException("Function指定的类必须继承自IFunction");
                if(value.GetConstructors().Length != 1) throw new Exception("Function指定的类只能有1个构造函数");
                functions = value;
            }
        }

        public bool IsMatch(string message) {
            var endIndex = URI.IndexOf("{");
            if(endIndex == -1) endIndex = URI.Length - 1;
            return message.StartsWith(URI.Substring(0, endIndex));
        }

        public IFunction GetFunctionInstance(CQGroupMessageEventArgs e) {
            var parameterDictionary = GetParameterDictionary(e.Message.Text);
            if(parameterDictionary == null) return null;

            var parameters = new List<object>();
            var constructor = Function.GetConstructors()[0];
            foreach(var p in constructor.GetParameters()) {
                if(p.ParameterType == typeof(Group)) parameters.Add(e.FromGroup);
                else if(p.ParameterType == typeof(QQ)) parameters.Add(e.FromQQ);
                else if(p.ParameterType == typeof(QQMessage)) parameters.Add(e.Message);
                else if(p.ParameterType == typeof(CQApi)) parameters.Add(e.CQApi);
                else if(p.ParameterType == typeof(CQLog)) parameters.Add(e.CQLog);
                else if(p.ParameterType == typeof(CQGroupMessageEventArgs)) parameters.Add(e);
                else if(p.ParameterType == typeof(string) && parameterDictionary.TryGetValue(p.Name.ToLower(), out string value)) parameters.Add(value);
                else parameters.Add(null);
            }
            return (IFunction)constructor.Invoke(parameters.ToArray());
        }

        private Dictionary<string, string> GetParameterDictionary(string message) {
            var result = new Dictionary<string, string>();
            var startIndex = URI.IndexOf("{");
            if(startIndex == -1) return result;
            message = new Regex("\\[CQ:at,\\s*qq=(.+?)\\]").Replace(message, " $1 ");
            var msg_param = message.Substring(startIndex).Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            if(msg_param.Length == 0) return null;
            var uri_param = Regex.Matches(URI, @"{(.+?)}");
            var index = 1;
            foreach(Match m in uri_param) {
                var p = m.Groups[1].Value;
                if(msg_param.Length < index) return result;
                result.Add(p.ToLower(), msg_param[index - 1]);
                index++;
            }
            return result;
        }
    }
}
