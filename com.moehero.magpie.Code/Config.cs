using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace com.moehero.cuckoo.Code
{
    //TODO 重构
    internal static class Config
    {
        private static IniSection _ini;
        private static IniObject _iniObject;

        private static void InitConfig() {
            var path = AppDirectory + "Config.ini";
            if(!File.Exists(path)) File.WriteAllText(path, "");
            _iniObject = IniObject.Load(path);
            _ini = _iniObject.Find(s => s.Name == "Application") ?? new IniSection("Application");

            EnabledGroups = GetValue<long>("EnabledGroups");
        }

        public static long OwnerNumber { get; } = 562416714;

        private static string _appDirectory;

        public static string AppDirectory {
            get => _appDirectory;
            set {
                _appDirectory = value;
                InitConfig();
            }
        }

        /// <summary>
        /// 机器人开关
        /// </summary>
        internal static bool Enabled {
            get => GetValue(true);
            set => SetValue(value);
        }

        private static ObservableCollection<long> _enabledGruops;

        /// <summary>
        /// 群启用列表
        /// </summary>
        internal static ObservableCollection<long> EnabledGroups {
            get {
                if(_enabledGruops == null) EnabledGroups = new ObservableCollection<long>();
                return _enabledGruops;
            }
            set {
                if(_enabledGruops != null) _enabledGruops.CollectionChanged -= CollectionChanged;
                _enabledGruops = value;
                SetValue(_enabledGruops, "EnabledGroups");
                _enabledGruops.CollectionChanged += CollectionChanged;

                void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                    SetValue(_enabledGruops, "EnabledGroups");
                }
            }
        }

        private static ObservableCollection<long> _admins;

        /// <summary>
        /// 管理员列表
        /// </summary>
        internal static ObservableCollection<long> Admins {
            get {
                if(_admins == null) Admins = new ObservableCollection<long>();
                return _admins;
            }
            set {
                if(_admins != null) _admins.CollectionChanged -= CollectionChanged;
                _admins = value;
                SetValue(_admins, "Admins");
                _admins.CollectionChanged += CollectionChanged;

                void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                    SetValue(_admins, "Admins");
                }
            }
        }

        #region 获取&设置

        private static string GetValue(string defaultValue = "", [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            return _ini[key]?.ToString() ?? defaultValue;
        }

        private static bool GetValue(bool defaultValue = false, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            if(!bool.TryParse(GetValue("", key), out bool value)) return defaultValue;
            return value;
        }

        private static long GetValue(long defaultValue = 0, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            if(!long.TryParse(GetValue("", key), out long value)) return defaultValue;
            return value;
        }

        private static ushort GetValue(ushort defaultValue = 0, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            if(!ushort.TryParse(GetValue("", key), out ushort value)) return defaultValue;
            return value;
        }

        private static ObservableCollection<T> GetValue<T>(string key) {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            var value = GetValue("", key).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(value.Length == 0) return new ObservableCollection<T>();
            var values = Array.ConvertAll(value, v => (T)Convert.ChangeType(v, typeof(T)));
            return new ObservableCollection<T>(values);
        }

        private static void SetValue(string value, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            if(_ini.ContainsKey(key)) _ini[key] = new IniValue(value);
            else _ini.Add(key, value);
            //保存
            if(_iniObject.Exists(s => s.Name == "Application")) _iniObject["Application"] = _ini;
            else _iniObject.Add(_ini);
            _iniObject.Save();
        }

        private static void SetValue(bool value, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            SetValue(value.ToString(), key);
        }

        private static void SetValue(long value, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            SetValue(value.ToString(), key);
        }

        private static void SetValue(ushort value, [CallerMemberName] string key = "") {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            SetValue(value.ToString(), key);
        }

        private static void SetValue<T>(ObservableCollection<T> value, string key) {
            if(string.IsNullOrEmpty(key)) throw new NotImplementedException("动态获取Key失败");
            SetValue(value.Aggregate("", (current, p) => $"{current}{p},").TrimEnd(','), key);
        }

        #endregion
    }
}
