using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LiteMonitor.src.Core;
using LiteMonitor.src.SystemServices;
using LiteMonitor.src.UI;

namespace LiteMonitor.src.System
{
    /// <summary>
    /// 热键管理器 - 负责全局和应用级快捷键的处理
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        private readonly Settings _cfg;
        private readonly MainForm _mainForm;
        private readonly UIController? _uiController;
        
        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
        
        // 热键ID管理
        private readonly Dictionary<int, HotkeyConfig> _registeredHotkeys = new();
        private int _nextHotkeyId = 0x0000;
        
        // 是否已初始化
        private bool _initialized = false;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public HotkeyManager(Settings cfg, MainForm mainForm, UIController? uiController = null)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _uiController = uiController;
        }
        
        /// <summary>
        /// 初始化热键管理器
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            
            try
            {
                // 注册全局热键
                RegisterGlobalHotkeys();
                
                // 设置应用级键盘事件
                _mainForm.KeyPreview = true;
                _mainForm.KeyDown += OnAppKeyDown;
                
                _initialized = true;
                Logger.Info("热键管理器初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error($"热键管理器初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 注册所有启用的全局热键
        /// </summary>
        private void RegisterGlobalHotkeys()
        {
            if (_cfg.Hotkeys == null) return;
            
            foreach (var hotkey in _cfg.Hotkeys.Where(h => h.Enabled && h.IsGlobal && h.IsValid))
            {
                try
                {
                    uint modifiers = ConvertModifiers(hotkey.Modifiers);
                    uint vk = (uint)hotkey.Key;
                    
                    // 生成唯一的热键ID
                    int hotkeyId = _nextHotkeyId++;
                    
                    if (RegisterHotKey(_mainForm.Handle, hotkeyId, modifiers, vk))
                    {
                        _registeredHotkeys[hotkeyId] = hotkey;
                        Logger.Debug($"注册全局热键成功: {hotkey.Description} ({hotkey.DisplayString})");
                    }
                    else
                    {
                        Logger.Warn($"注册全局热键失败: {hotkey.Description} ({hotkey.DisplayString}) - 可能与其他程序冲突");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"注册热键时出错 {hotkey.Description}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 将ModifierKeys枚举转换为Windows API的modifier值
        /// </summary>
        private uint ConvertModifiers(ModifierKeys modifiers)
        {
            uint result = 0;
            
            if ((modifiers & ModifierKeys.Alt) != 0) result |= 0x0001; // MOD_ALT
            if ((modifiers & ModifierKeys.Control) != 0) result |= 0x0002; // MOD_CONTROL
            if ((modifiers & ModifierKeys.Shift) != 0) result |= 0x0004; // MOD_SHIFT
            if ((modifiers & ModifierKeys.Win) != 0) result |= 0x0008; // MOD_WIN
            
            return result;
        }
        
        /// <summary>
        /// 获取当前按下的修饰键
        /// </summary>
        private ModifierKeys GetPressedModifiers()
        {
            ModifierKeys modifiers = ModifierKeys.None;
            
            if (Control.ModifierKeys.HasFlag(Keys.Control)) modifiers |= ModifierKeys.Control;
            if (Control.ModifierKeys.HasFlag(Keys.Alt)) modifiers |= ModifierKeys.Alt;
            if (Control.ModifierKeys.HasFlag(Keys.Shift)) modifiers |= ModifierKeys.Shift;
            
            // Win键需要特殊处理
            if ((GetAsyncKeyState(Keys.LWin) & 0x8000) != 0 || (GetAsyncKeyState(Keys.RWin) & 0x8000) != 0)
            {
                modifiers |= ModifierKeys.Win;
            }
            
            return modifiers;
        }
        
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        
        /// <summary>
        /// 处理应用级键盘事件
        /// </summary>
        private void OnAppKeyDown(object? sender, KeyEventArgs e)
        {
            if (_cfg.Hotkeys == null) return;
            
            var pressedModifiers = GetPressedModifiers();
            var pressedKey = e.KeyCode;
            
            // 查找匹配的应用级快捷键
            var hotkey = _cfg.Hotkeys.FirstOrDefault(h => 
                h.Enabled && !h.IsGlobal && h.IsValid &&
                h.Key == pressedKey && 
                h.Modifiers == pressedModifiers);
                
            if (hotkey != null)
            {
                ExecuteAction(hotkey.ActionId);
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// 处理Windows消息（用于全局热键）
        /// </summary>
        public void ProcessMessage(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (_registeredHotkeys.TryGetValue(id, out var hotkey))
                {
                    ExecuteAction(hotkey.ActionId);
                }
            }
        }
        
        /// <summary>
        /// 执行快捷键动作
        /// </summary>
        private void ExecuteAction(string actionId)
        {
            try
            {
                Logger.Debug($"执行快捷键动作: {actionId}");
                
                if (Enum.TryParse<HotkeyAction>(actionId, out var action))
                {
                    switch (action)
                    {
                        case HotkeyAction.ToggleVisibility:
                            ToggleVisibility();
                            break;
                            
                        case HotkeyAction.ToggleLayoutMode:
                            ToggleLayoutMode();
                            break;
                            
                        case HotkeyAction.ToggleClickThrough:
                            ToggleClickThrough();
                            break;
                            
                        case HotkeyAction.ToggleTaskbar:
                            ToggleTaskbar();
                            break;
                            
                        case HotkeyAction.CleanMemory:
                            CleanMemory();
                            break;
                            
                        case HotkeyAction.ToggleTrayIcon:
                            ToggleTrayIcon();
                            break;
                            
                        default:
                            Logger.Warn($"未知的快捷键动作: {actionId}");
                            break;
                    }
                }
                else
                {
                    Logger.Warn($"无法解析的快捷键动作ID: {actionId}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"执行快捷键动作失败 {actionId}: {ex.Message}");
            }
        }
        
        // ====== 具体动作实现 ======
        
        private void ToggleVisibility()
        {
            if (_mainForm.Visible)
            {
                _mainForm.Hide();
                Logger.Info("隐藏主窗口");
            }
            else
            {
                _mainForm.Show();
                _mainForm.BringToFront();
                Logger.Info("显示主窗口");
            }
        }
        
        private void ToggleLayoutMode()
        {
            _cfg.HorizontalMode = !_cfg.HorizontalMode;
            _cfg.Save();
            
            // 调用现有的布局切换逻辑
            if (_uiController != null)
            {
                AppActions.ApplyThemeAndLayout(_cfg, _uiController, _mainForm);
            }
            
            Logger.Info($"切换显示模式: {(_cfg.HorizontalMode ? "横屏" : "竖屏")}");
        }
        
        private void ToggleClickThrough()
        {
            _cfg.ClickThrough = !_cfg.ClickThrough;
            _cfg.Save();
            
            // 应用鼠标穿透设置
            AppActions.ApplyWindowAttributes(_cfg, _mainForm);
            
            Logger.Info($"切换鼠标穿透: {(_cfg.ClickThrough ? "启用" : "禁用")}");
        }
        
        private void ToggleTaskbar()
        {
            _cfg.ShowTaskbar = !_cfg.ShowTaskbar;
            _cfg.Save();
            
            // 应用任务栏显示设置
            AppActions.ApplyVisibility(_cfg, _mainForm);
            
            Logger.Info($"切换任务栏显示: {(_cfg.ShowTaskbar ? "显示" : "隐藏")}");
        }
        
        private void CleanMemory()
        {
            // 调用现有的清理内存功能
            _mainForm.CleanMemory();
            Logger.Info("执行内存清理");
        }
        
        private void ToggleTrayIcon()
        {
            _cfg.HideTrayIcon = !_cfg.HideTrayIcon;
            _cfg.Save();
            
            // 应用托盘图标设置
            if (_cfg.HideTrayIcon)
            {
                _mainForm.HideTrayIcon();
            }
            else
            {
                _mainForm.ShowTrayIcon();
            }
            
            Logger.Info($"切换托盘图标: {(_cfg.HideTrayIcon ? "隐藏" : "显示")}");
        }
        
        /// <summary>
        /// 重新加载热键配置
        /// </summary>
        public void Reload()
        {
            try
            {
                // 注销所有已注册的全局热键
                UnregisterAllHotkeys();
                
                // 重新注册全局热键
                RegisterGlobalHotkeys();
                
                Logger.Info("热键配置重新加载完成");
            }
            catch (Exception ex)
            {
                Logger.Error($"重新加载热键配置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 注销所有全局热键
        /// </summary>
        private void UnregisterAllHotkeys()
        {
            foreach (var id in _registeredHotkeys.Keys.ToList())
            {
                try
                {
                    UnregisterHotKey(_mainForm.Handle, id);
                    _registeredHotkeys.Remove(id);
                }
                catch (Exception ex)
                {
                    Logger.Error($"注销热键失败 (ID: {id}): {ex.Message}");
                }
            }
            
            _nextHotkeyId = 0x0000;
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_initialized)
                {
                    // 移除事件处理器
                    _mainForm.KeyDown -= OnAppKeyDown;
                    
                    // 注销所有全局热键
                    UnregisterAllHotkeys();
                    
                    _initialized = false;
                    Logger.Info("热键管理器已释放");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"释放热键管理器时出错: {ex.Message}");
            }
        }
        
        // ====== 静态辅助方法 ======
        
        /// <summary>
        /// 将Keys转换为可显示的字符串
        /// </summary>
        public static string KeyToString(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9) return key.ToString().Substring(1);
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return "Num" + (key - Keys.NumPad0);
            
            // 特殊键处理
            return key switch
            {
                Keys.Oemcomma => ",",
                Keys.OemPeriod => ".",
                Keys.OemQuestion => "/",
                Keys.Oem1 => ";",
                Keys.Oem7 => "'",
                Keys.OemOpenBrackets => "[",
                Keys.OemCloseBrackets => "]",
                Keys.OemBackslash => "\\",
                Keys.OemMinus => "-",
                Keys.OemPlus => "=",
                _ => key.ToString()
            };
        }
        
        /// <summary>
        /// 检查热键是否可用（无冲突）
        /// </summary>
        public static bool IsHotkeyAvailable(Keys key, ModifierKeys modifiers, bool isGlobal, out string conflictReason)
        {
            conflictReason = null;
            
            if (key == Keys.None)
            {
                conflictReason = "未选择按键";
                return false;
            }
            
            // 禁止使用某些系统保留键
            var reservedKeys = new[] 
            { 
                Keys.Escape, Keys.Enter, Keys.Tab, Keys.CapsLock, 
                Keys.NumLock, Keys.Scroll, Keys.Pause, Keys.PrintScreen,
                Keys.Insert, Keys.Delete, Keys.Home, Keys.End,
                Keys.PageUp, Keys.PageDown
            };
            
            if (reservedKeys.Contains(key))
            {
                conflictReason = $"按键 {key} 是系统保留键";
                return false;
            }
            
            // 检查Windows系统保留快捷键
            if (isGlobal)
            {
                // Windows + 字母键是系统保留
                if ((modifiers & ModifierKeys.Win) != 0)
                {
                    var winReservedKeys = new[] 
                    { 
                        Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4,
                        Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
                        Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F,
                        Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L,
                        Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R,
                        Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X,
                        Keys.Y, Keys.Z
                    };
                    
                    if (winReservedKeys.Contains(key))
                    {
                        conflictReason = $"Win + {key} 是Windows系统保留快捷键";
                        return false;
                    }
                }
                
                // 检查其他常见系统快捷键
                if (key == Keys.F1 && modifiers == ModifierKeys.None)
                {
                    conflictReason = "F1 是Windows帮助快捷键";
                    return false;
                }
                
                if (key == Keys.F3 && modifiers == ModifierKeys.None)
                {
                    conflictReason = "F3 是Windows搜索快捷键";
                    return false;
                }
                
                if (key == Keys.F5 && modifiers == ModifierKeys.None)
                {
                    conflictReason = "F5 是刷新快捷键";
                    return false;
                }
                
                if (key == Keys.F11 && modifiers == ModifierKeys.None)
                {
                    conflictReason = "F11 是全屏切换快捷键";
                    return false;
                }
                
                if (key == Keys.F12 && modifiers == ModifierKeys.None)
                {
                    // F12通常用于开发者工具，但允许使用
                    // 只是给出警告
                    conflictReason = "F12 通常用于浏览器开发者工具";
                    // 不返回false，只是警告
                }
            }
            
            // 检查内部冲突（相同快捷键重复设置）
            // 这个需要在具体实例中检查，这里只提供方法
            
            return true;
        }
        
        /// <summary>
        /// 检查热键是否可用（简化版）
        /// </summary>
        public static bool IsHotkeyAvailable(Keys key, ModifierKeys modifiers, bool isGlobal)
        {
            return IsHotkeyAvailable(key, modifiers, isGlobal, out _);
        }
        
        /// <summary>
        /// 检测热键冲突并返回冲突信息
        /// </summary>
        public List<string> DetectConflicts(List<HotkeyConfig> hotkeys)
        {
            var conflicts = new List<string>();
            
            if (hotkeys == null || hotkeys.Count == 0)
                return conflicts;
            
            // 检查重复的热键
            var hotkeyMap = new Dictionary<string, List<string>>();
            
            foreach (var hotkey in hotkeys.Where(h => h.Enabled && h.IsValid))
            {
                string key = $"{hotkey.Modifiers}|{hotkey.Key}";
                
                if (!hotkeyMap.ContainsKey(key))
                    hotkeyMap[key] = new List<string>();
                
                hotkeyMap[key].Add(hotkey.Description);
            }
            
            // 找出重复的热键
            foreach (var kvp in hotkeyMap)
            {
                if (kvp.Value.Count > 1)
                {
                    conflicts.Add($"快捷键冲突: {string.Join(", ", kvp.Value)} 使用了相同的按键组合");
                }
            }
            
            // 检查系统保留键
            foreach (var hotkey in hotkeys.Where(h => h.Enabled && h.IsValid))
            {
                if (!IsHotkeyAvailable(hotkey.Key, hotkey.Modifiers, hotkey.IsGlobal, out string reason))
                {
                    conflicts.Add($"{hotkey.Description}: {reason}");
                }
            }
            
            return conflicts;
        }
        
        /// <summary>
        /// 显示冲突提示对话框
        /// </summary>
        public static void ShowConflictDialog(List<string> conflicts, Control parent = null)
        {
            if (conflicts == null || conflicts.Count == 0)
                return;
            
            string message = "检测到以下快捷键冲突：\n\n";
            message += string.Join("\n", conflicts.Select(c => $"• {c}"));
            message += "\n\n建议修改冲突的快捷键设置。";
            
            MessageBox.Show(
                message,
                "快捷键冲突检测",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                parent == null ? 0 : (MessageBoxOptions)0x40000); // MB_TOPMOST if parent is null
        }
    }
    
    /// <summary>
    /// 简单的日志记录类
    /// </summary>
    internal static class Logger
    {
        public static void Debug(string message) => Write("DEBUG", message);
        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message) => Write("ERROR", message);
        
        private static void Write(string level, string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
            
            // 在实际项目中，这里应该写入文件或系统日志
            // 为了简化，暂时只输出到控制台
        }
    }
}