using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LiteMonitor.src.Core;
using LiteMonitor.src.UI.Controls;

namespace LiteMonitor.src.UI.SettingsPage
{
    public class HotkeyPage : SettingsPageBase
    {
        private Panel _container;
        private TableLayoutPanel _hotkeyTable;
        private List<HotkeyRowControl> _hotkeyRows = new List<HotkeyRowControl>();
        
        public HotkeyPage()
        {
            this.BackColor = UIColors.MainBg;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0);
            
            _container = new BufferedPanel 
            { 
                Dock = DockStyle.Fill, 
                AutoScroll = true, 
                Padding = new Padding(20) 
            };
            
            this.Controls.Add(_container);
            
            // 立即构建UI
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 清空容器
            _container.Controls.Clear();
            _hotkeyRows.Clear();
            
            // 创建标题
            var titleLabel = new Label
            {
                Text = LanguageManager.T("HotkeySettings"),
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
                ForeColor = UIColors.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            _container.Controls.Add(titleLabel);
            
            // 创建说明标签
            var tipLabel = new Label
            {
                Text = LanguageManager.T("HotkeyTips.GlobalHotkeyTip") + "\n" + 
                       LanguageManager.T("HotkeyTips.AppHotkeyTip"),
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = UIColors.TextSecondary,
                AutoSize = true,
                MaximumSize = new Size(_container.Width - 40, 0),
                Margin = new Padding(0, 0, 0, 20)
            };
            _container.Controls.Add(tipLabel);
            
            // 创建快捷键表格
            CreateHotkeyTable();
            
            // 创建操作按钮组
            CreateActionButtons();
        }
        
        private void CreateHotkeyTable()
        {
            // 创建表格
            _hotkeyTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 6,
                RowCount = 1, // 第一行是表头
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Margin = new Padding(0, 0, 0, 20)
            };
            
            // 设置列宽
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); // 描述
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); // 按键
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); // 修饰键
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10)); // 启用
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); // 全局热键
            _hotkeyTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // 操作
            
            // 添加表头
            AddTableHeader();
            
            // 添加快捷键行
            if (Config?.Hotkeys != null)
            {
                foreach (var hotkey in Config.Hotkeys)
                {
                    AddHotkeyRow(hotkey);
                }
            }
            
            _container.Controls.Add(_hotkeyTable);
        }
        
        private void AddTableHeader()
        {
            var headers = new[]
            {
                LanguageManager.T("HotkeyDescription"),
                LanguageManager.T("HotkeyKey"),
                LanguageManager.T("HotkeyModifiers"),
                LanguageManager.T("HotkeyEnabled"),
                LanguageManager.T("HotkeyGlobal"),
                "" // 操作列不需要标题
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var headerLabel = new Label
                {
                    Text = headers[i],
                    Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
                    ForeColor = UIColors.TextPrimary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5)
                };
                
                _hotkeyTable.Controls.Add(headerLabel, i, 0);
            }
        }
        
        private void AddHotkeyRow(HotkeyConfig hotkey)
        {
            int rowIndex = _hotkeyTable.RowCount;
            _hotkeyTable.RowCount++;
            _hotkeyTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            
            // 创建行控件
            var rowControl = new HotkeyRowControl(hotkey, this);
            _hotkeyRows.Add(rowControl);
            
            // 将控件添加到表格
            for (int i = 0; i < 6; i++)
            {
                var control = rowControl.GetCellControl(i);
                if (control != null)
                {
                    _hotkeyTable.Controls.Add(control, i, rowIndex);
                }
            }
        }
        
        private void CreateActionButtons()
        {
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 10)
            };
            
            // 重置按钮
            var resetButton = new Button
            {
                Text = LanguageManager.T("HotkeyReset"),
                Font = new Font("Microsoft YaHei UI", 9),
                Padding = new Padding(10, 5),
                AutoSize = true,
                BackColor = UIColors.ButtonNormal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            resetButton.FlatAppearance.BorderSize = 0;
            resetButton.Click += (s, e) => ResetToDefaults();
            
            // 测试按钮
            var testButton = new Button
            {
                Text = LanguageManager.T("HotkeyTest"),
                Font = new Font("Microsoft YaHei UI", 9),
                Padding = new Padding(10, 5),
                AutoSize = true,
                BackColor = UIColors.ButtonNormal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            testButton.FlatAppearance.BorderSize = 0;
            testButton.Click += (s, e) => TestHotkeys();
            
            buttonPanel.Controls.Add(resetButton);
            buttonPanel.Controls.Add(testButton);
            
            _container.Controls.Add(buttonPanel);
        }
        
        private void ResetToDefaults()
        {
            if (Config == null) return;
            
            var result = MessageBox.Show(
                LanguageManager.T("HotkeyTips.DefaultTip"),
                LanguageManager.T("HotkeyReset"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                Config.InitDefaultHotkeys();
                RefreshUI();
                
                MessageBox.Show(
                    LanguageManager.T("HotkeyMessages.Reset"),
                    LanguageManager.T("HotkeyReset"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        
        private void TestHotkeys()
        {
            MessageBox.Show(
                "测试快捷键功能：\n\n" +
                "1. F12 - 显示/隐藏主窗口（全局）\n" +
                "2. F11 - 切换任务栏显示（全局）\n" +
                "3. F5 - 切换显示模式（应用级）\n" +
                "4. Ctrl+P - 切换鼠标穿透（应用级）\n" +
                "5. Ctrl+M - 清理内存（应用级）\n" +
                "6. Ctrl+Y - 切换托盘图标（应用级）\n\n" +
                "注意：应用级快捷键需要设置窗口激活状态。",
                "快捷键测试",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        public void RefreshUI()
        {
            InitializeUI();
        }
        
        public override void OnShow()
        {
            base.OnShow();
            RefreshUI();
        }
        
        public override void Save()
        {
            base.Save();
            
            // 保存所有快捷键行的配置
            foreach (var row in _hotkeyRows)
            {
                row.SaveToConfig();
            }
            
            // 检查冲突
            if (Config?.Hotkeys != null)
            {
                var hotkeyManager = new HotkeyManager(Config, MainForm, UI);
                var conflicts = hotkeyManager.DetectConflicts(Config.Hotkeys);
                
                if (conflicts.Count > 0)
                {
                    HotkeyManager.ShowConflictDialog(conflicts, this);
                    
                    // 询问用户是否继续
                    var result = MessageBox.Show(
                        "检测到快捷键冲突。是否继续保存？\n\n冲突的快捷键可能无法正常工作。",
                        "快捷键冲突",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.No)
                    {
                        return; // 不保存
                    }
                }
            }
            
            // 重新加载热键
            MainForm?.ReloadHotkeys();
            
            // 显示保存成功消息
            MessageBox.Show(
                LanguageManager.T("HotkeyMessages.Saved"),
                LanguageManager.T("HotkeySettings"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
    
    /// <summary>
    /// 快捷键行控件
    /// </summary>
    internal class HotkeyRowControl
    {
        private HotkeyConfig _hotkey;
        private HotkeyPage _page;
        
        private Label _descriptionLabel;
        private TextBox _keyTextBox;
        private ComboBox _modifierComboBox;
        private CheckBox _enabledCheckBox;
        private CheckBox _globalCheckBox;
        private Button _recordButton;
        
        public HotkeyRowControl(HotkeyConfig hotkey, HotkeyPage page)
        {
            _hotkey = hotkey;
            _page = page;
            InitializeControls();
        }
        
        private void InitializeControls()
        {
            // 描述标签
            _descriptionLabel = new Label
            {
                Text = GetDescriptionText(),
                Font = new Font("Microsoft YaHei UI", 9),
                ForeColor = UIColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                AutoSize = false
            };
            
            // 按键文本框
            _keyTextBox = new TextBox
            {
                Text = _hotkey.Key.ToString(),
                Font = new Font("Microsoft YaHei UI", 9),
                TextAlign = HorizontalAlignment.Center,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = SystemColors.Window
            };
            
            // 修饰键下拉框
            _modifierComboBox = new ComboBox
            {
                Font = new Font("Microsoft YaHei UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            
            // 添加修饰键选项
            var modifiers = Enum.GetValues(typeof(ModifierKeys)).Cast<ModifierKeys>();
            foreach (var modifier in modifiers)
            {
                string displayText = GetModifierDisplayText(modifier);
                _modifierComboBox.Items.Add(new ModifierItem(modifier, displayText));
            }
            
            // 设置当前选中的修饰键
            foreach (ModifierItem item in _modifierComboBox.Items)
            {
                if (item.Modifier == _hotkey.Modifiers)
                {
                    _modifierComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 启用复选框
            _enabledCheckBox = new CheckBox
            {
                Checked = _hotkey.Enabled,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            
            // 全局热键复选框
            _globalCheckBox = new CheckBox
            {
                Checked = _hotkey.IsGlobal,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            
            // 录制按钮
            _recordButton = new Button
            {
                Text = LanguageManager.T("HotkeyRecord"),
                Font = new Font("Microsoft YaHei UI", 9),
                Padding = new Padding(5),
                AutoSize = true,
                BackColor = UIColors.ButtonNormal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _recordButton.FlatAppearance.BorderSize = 0;
            _recordButton.Click += OnRecordButtonClick;
            
            // 注册保存动作
            _page.RegisterDelaySave(SaveToConfig);
        }
        
        private string GetDescriptionText()
        {
            // 尝试从语言文件中获取描述
            string actionKey = $"HotkeyActions.{_hotkey.ActionId}";
            string translated = LanguageManager.T(actionKey);
            
            if (translated != actionKey) // 如果找到了翻译
            {
                return translated;
            }
            
            // 否则使用配置中的描述
            return _hotkey.Description;
        }
        
        private string GetModifierDisplayText(ModifierKeys modifier)
        {
            string modifierKey = $"HotkeyModifierNames.{modifier}";
            string translated = LanguageManager.T(modifierKey);
            
            if (translated != modifierKey)
            {
                return translated;
            }
            
            return modifier.ToString();
        }
        
        private void OnRecordButtonClick(object sender, EventArgs e)
        {
            var recordForm = new HotkeyRecordForm(_hotkey);
            if (recordForm.ShowDialog() == DialogResult.OK)
            {
                _hotkey.Key = recordForm.RecordedKey;
                _hotkey.Modifiers = recordForm.RecordedModifiers;
                
                // 更新UI
                _keyTextBox.Text = _hotkey.Key.ToString();
                
                foreach (ModifierItem item in _modifierComboBox.Items)
                {
                    if (item.Modifier == _hotkey.Modifiers)
                    {
                        _modifierComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }
        
        public Control GetCellControl(int columnIndex)
        {
            return columnIndex switch
            {
                0 => _descriptionLabel,
                1 => _keyTextBox,
                2 => _modifierComboBox,
                3 => _enabledCheckBox,
                4 => _globalCheckBox,
                5 => _recordButton,
                _ => null
            };
        }
        
        public void SaveToConfig()
        {
            if (_hotkey == null) return;
            
            _hotkey.Enabled = _enabledCheckBox.Checked;
            _hotkey.IsGlobal = _globalCheckBox.Checked;
            
            if (_modifierComboBox.SelectedItem is ModifierItem selectedModifier)
            {
                _hotkey.Modifiers = selectedModifier.Modifier;
            }
        }
        
        private class ModifierItem
        {
            public ModifierKeys Modifier { get; }
            public string DisplayText { get; }
            
            public ModifierItem(ModifierKeys modifier, string displayText)
            {
                Modifier = modifier;
                DisplayText = displayText;
            }
            
            public override string ToString() => DisplayText;
        }
    }
    
    /// <summary>
    /// 快捷键录制窗体
    /// </summary>
    internal class HotkeyRecordForm : Form
    {
        public Keys RecordedKey { get; private set; }
        public ModifierKeys RecordedModifiers { get; private set; }
        
        private Label _instructionLabel;
        private Label _keyLabel;
        private Button _okButton;
        private Button _cancelButton;
        private HotkeyConfig _originalHotkey;
        
        public HotkeyRecordForm(HotkeyConfig hotkey)
        {
            _originalHotkey = hotkey;
            RecordedKey = hotkey.Key;
            RecordedModifiers = hotkey.Modifiers;
            
            InitializeForm();
            UpdateKeyDisplay();
        }
        
        private void InitializeForm()
        {
            this.Text = LanguageManager.T("HotkeyRecord");
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            
            // 指令标签
            _instructionLabel = new Label
            {
                Text = LanguageManager.T("HotkeyTips.RecordTip"),
                Font = new Font("Microsoft YaHei UI", 10),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 60,
                Padding = new Padding(10)
            };
            
            // 按键显示标签
            _keyLabel = new Label
            {
                Text = "",
                Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // 按钮面板
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };
            
            // 确定按钮
            _okButton = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Width = 80
            };
            
            // 取消按钮
            _cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Width = 80
            };
            
            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);
            
            // 添加控件
            this.Controls.Add(_keyLabel);
            this.Controls.Add(_instructionLabel);
            this.Controls.Add(buttonPanel);
            
            // 键盘事件
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // 忽略功能键的单独按下
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || 
                e.KeyCode == Keys.Menu || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                return;
            }
            
            RecordedKey = e.KeyCode;
            RecordedModifiers = GetPressedModifiers(e);
            
            // 检查冲突
            CheckForConflicts();
            
            UpdateKeyDisplay();
            e.Handled = true;
        }
        
        private void CheckForConflicts()
        {
            // 检查是否为有效热键
            bool isValid = HotkeyManager.IsHotkeyAvailable(
                RecordedKey, RecordedModifiers, _originalHotkey.IsGlobal, out string conflictReason);
            
            if (!isValid && !string.IsNullOrEmpty(conflictReason))
            {
                // 显示冲突警告，但不阻止用户使用
                _keyLabel.ForeColor = Color.Red;
                _keyLabel.Text += $" ({conflictReason})";
            }
            else
            {
                _keyLabel.ForeColor = SystemColors.ControlText;
            }
        }
        
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // 如果用户按下了ESC，取消录制
            if (e.KeyCode == Keys.Escape)
            {
                RecordedKey = _originalHotkey.Key;
                RecordedModifiers = _originalHotkey.Modifiers;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
        
        private ModifierKeys GetPressedModifiers(KeyEventArgs e)
        {
            ModifierKeys modifiers = ModifierKeys.None;
            
            if (e.Control) modifiers |= ModifierKeys.Control;
            if (e.Alt) modifiers |= ModifierKeys.Alt;
            if (e.Shift) modifiers |= ModifierKeys.Shift;
            
            // 检查Win键
            if ((GetAsyncKeyState(Keys.LWin) & 0x8000) != 0 || 
                (GetAsyncKeyState(Keys.RWin) & 0x8000) != 0)
            {
                modifiers |= ModifierKeys.Win;
            }
            
            return modifiers;
        }
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        
        private void UpdateKeyDisplay()
        {
            var parts = new List<string>();
            
            if ((RecordedModifiers & ModifierKeys.Control) != 0) parts.Add("Ctrl");
            if ((RecordedModifiers & ModifierKeys.Alt) != 0) parts.Add("Alt");
            if ((RecordedModifiers & ModifierKeys.Shift) != 0) parts.Add("Shift");
            if ((RecordedModifiers & ModifierKeys.Win) != 0) parts.Add("Win");
            
            if (RecordedKey != Keys.None && RecordedKey != Keys.None)
            {
                parts.Add(RecordedKey.ToString());
            }
            
            _keyLabel.Text = parts.Count > 0 ? string.Join(" + ", parts) : "(无按键)";
            
            // 更新确定按钮状态
            _okButton.Enabled = RecordedKey != Keys.None && RecordedKey != Keys.None;
        }
    }
}