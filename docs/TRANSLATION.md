# LiteMonitor 翻译系统文档

## 概述

LiteMonitor 使用基于 JSON 的翻译系统，支持多语言切换。系统采用字符串驻留（String Interning）优化内存使用，并支持运行时语言切换。

## 支持的语言

目前支持以下语言：

| 语言代码 | 语言名称 | 文件路径 |
|---------|---------|---------|
| zh | 简体中文 | `resources/lang/zh.json` |
| en | English | `resources/lang/en.json` |
| zh-Hant | 繁體中文 | `resources/lang/zh-Hant.json` |
| ja | 日本語 | `resources/lang/ja.json` |
| ko | 한국어 | `resources/lang/ko.json` |
| de | Deutsch | `resources/lang/de.json` |
| fr | Français | `resources/lang/fr.json` |
| ru | Русский | `resources/lang/ru.json` |

## 翻译文件结构

### 基本结构

```json
{
  "Title": "⚡️LiteMonitor",
  "Groups": {
    "DASH": "ℹ️看板",
    "CPU": "💻CPU"
  },
  "Items": {
    "CPU.Load": "CPU使用率",
    "CPU.Temp": "CPU温度"
  },
  "Short": {
    "CPU.Load": "负载"
  },
  "Units": {
    "Percentage": "%",
    "Temperature": "°C"
  }
}
```

### 命名空间约定

翻译键使用以下命名空间：

1. **Groups** - 监控项分组名称
   - `DASH` - 看板/系统信息
   - `CPU` - CPU信息
   - `GPU` - GPU信息
   - `HOST` - 主机信息
   - `DISK` - 磁盘信息
   - `NET` - 网络信息
   - `DATA` - 流量统计
   - `BAT` - 电池信息

2. **Items** - 监控项完整名称
   - 格式: `GroupName.ItemName`
   - 示例: `CPU.Load`, `GPU.Temp`

3. **Short** - 监控项短名称（用于紧凑显示）
   - 格式与 Items 相同
   - 示例: `CPU.Load` → "负载"

4. **Units** - 单位符号
   - `Percentage` - %
   - `Temperature` - °C/°F
   - `Speed` - Mbps/Gbps

5. **Settings** - 设置界面文本
6. **HotkeyActions** - 热键动作名称
7. **HotkeyMessages** - 热键相关提示信息
8. **Menu** - 菜单文本

## 如何使用翻译

### 在代码中使用

#### 基础用法

```csharp
using LiteMonitor.src.Core;

// 获取翻译文本
string cpuText = LanguageManager.T("Items.CPU.Load");
string groupName = LanguageManager.T("Groups.CPU");
```

#### 带参数的翻译

```csharp
// 翻译键: "FileSaved": "文件 {0} 已保存"
string message = LanguageManager.T("Messages.FileSaved", "example.txt");
// 结果: "文件 example.txt 已保存"
```

#### 获取当前语言

```csharp
string currentLang = LanguageManager.CurrentLanguage;
```

#### 切换语言

```csharp
// 切换到英语
LanguageManager.LoadLanguage("en");
```

### 在 UI 控件中使用

#### 自动更新绑定

```csharp
// 设置控件的文本并绑定语言切换事件
label.Text = LanguageManager.T("Items.CPU.Load");
LanguageManager.OnLanguageChanged += () => {
    label.Text = LanguageManager.T("Items.CPU.Load");
};
```

#### 使用扩展方法

```csharp
// 如果可用
label.SetLocalizedText("Items.CPU.Load");
```

## 添加新语言

### 步骤

1. **创建翻译文件**

   在 `resources/lang/` 目录下创建新的 JSON 文件：
   ```
   resources/lang/[语言代码].json
   ```

2. **复制基础结构**

   从 `en.json` 或 `zh.json` 复制内容作为模板。

3. **翻译所有文本**

   将所有的值翻译为目标语言，保持键名不变。

4. **注册语言**

   在 `LanguageManager.cs` 中的 `SupportedLanguages` 列表中添加：

   ```csharp
   public static readonly string[] SupportedLanguages = new[]
   {
       "zh", "en", "zh-Hant", "ja", "ko", 
       "de", "fr", "ru", "your-lang-code"
   };
   ```

5. **测试**

   运行程序，在设置中切换到新语言，检查所有文本是否正确显示。

### 翻译键命名规范

#### 通用规则

1. **使用 PascalCase** - 每个单词首字母大写
2. **使用点号分隔** - 表示层级关系
3. **保持简洁** - 键名应该简短且有意义
4. **避免特殊字符** - 只使用字母、数字和点号

#### 具体规范

**Groups 命名**
```
Groups.{CategoryName}

示例:
Groups.CPU      // CPU
Groups.GPU      // GPU
Groups.DISK     // 磁盘
```

**Items 命名**
```
Items.{Category}.{Metric}

示例:
Items.CPU.Load      // CPU使用率
Items.CPU.Temp      // CPU温度
Items.GPU.VRAM      // 显存占用
```

**Settings 命名**
```
Settings.{Section}.{Control}

示例:
Settings.General.Startup      // 开机启动
Settings.Display.Opacity      // 透明度
```

**HotkeyActions 命名**
```
HotkeyActions.{ActionName}

示例:
HotkeyActions.ToggleVisibility   // 显示/隐藏
HotkeyActions.CleanMemory        // 清理内存
```

**Messages 命名**
```
{Category}Messages.{MessageType}

示例:
HotkeyMessages.Saved        // 配置已保存
ErrorMessages.FileNotFound  // 文件未找到
```

## 最佳实践

### 1. 避免硬编码

❌ **错误：**
```csharp
label.Text = "CPU Usage";
```

✅ **正确：**
```csharp
label.Text = LanguageManager.T("Items.CPU.Load");
```

### 2. 使用有意义的键名

❌ **错误：**
```json
{
  "Msg1": "保存成功",
  "Msg2": "保存失败"
}
```

✅ **正确：**
```json
{
  "SaveSuccess": "保存成功",
  "SaveFailed": "保存失败"
}
```

### 3. 保持所有语言同步

添加新的翻译键时，同时更新所有语言文件。如果某个语言暂时没有翻译，可以先用英语占位：

```json
{
  "NewFeature": "New Feature"  // 待翻译
}
```

### 4. 使用参数化而不是拼接

❌ **错误：**
```csharp
string msg = LanguageManager.T("Saved") + filename;
```

✅ **正确：**
```csharp
string msg = LanguageManager.T("FileSaved", filename);
```

翻译文件：
```json
{
  "FileSaved": "文件 {0} 已保存"
}
```

### 5. 组织相关文本

将相关的翻译键组织在一起，使用有意义的前缀：

```json
{
  "Hotkey": {
    "Title": "快捷键设置",
    "Global": "全局快捷键",
    "App": "应用快捷键",
    "Record": "录制",
    "Clear": "清除"
  }
}
```

## 性能优化

### 字符串驻留

翻译系统使用字符串驻留（String Interning）技术来减少内存使用。重复的翻译文本只会存储一次。

### 缓存机制

- 翻译文件在首次加载时解析并缓存
- 语言切换时重新加载
- 运行时动态获取（不需要重启程序）

### 内存管理

- 翻译文本使用 `string` 类型存储
- 大型翻译文件不会显著增加内存占用（得益于字符串驻留）
- 建议在 UI 层缓存常用的翻译文本，避免重复调用

## 故障排除

### 翻译不显示

**问题：** 界面上显示翻译键而不是翻译文本

**解决方案：**
1. 检查翻译键是否拼写正确
2. 确认翻译文件存在于 `resources/lang/` 目录
3. 检查 JSON 文件格式是否正确（可以使用 JSON 验证器）
4. 查看程序日志是否有加载错误

### 语言切换无效

**问题：** 切换语言后界面没有更新

**解决方案：**
1. 确保订阅了 `OnLanguageChanged` 事件
2. 在事件处理程序中更新所有 UI 文本
3. 检查是否正确调用了 `LanguageManager.LoadLanguage()`

### 特殊字符显示异常

**问题：** 某些语言的字符显示为乱码

**解决方案：**
1. 确保 JSON 文件使用 UTF-8 编码保存
2. 检查字体是否支持该语言
3. 确认 Windows 系统已安装相应语言包

## 示例代码

### 完整的翻译使用示例

```csharp
using LiteMonitor.src.Core;
using System.Windows.Forms;

public class ExampleForm : Form
{
    private Label lblCpu;
    private Label lblGpu;
    private Button btnSave;
    
    public ExampleForm()
    {
        InitializeComponent();
        LoadTexts();
        
        // 订阅语言切换事件
        LanguageManager.OnLanguageChanged += LoadTexts;
    }
    
    private void LoadTexts()
    {
        // 监控项
        lblCpu.Text = LanguageManager.T("Items.CPU.Load");
        lblGpu.Text = LanguageManager.T("Items.GPU.Temp");
        
        // 按钮
        btnSave.Text = LanguageManager.T("Settings.Save");
        
        // 窗口标题
        this.Text = LanguageManager.T("Title");
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // 取消订阅避免内存泄漏
        LanguageManager.OnLanguageChanged -= LoadTexts;
        base.OnFormClosed(e);
    }
}
```

### 动态创建 UI 的翻译

```csharp
public void CreateMetricItem(string metricKey)
{
    var label = new Label();
    
    // 获取分组和监控项名称
    string groupName = LanguageManager.T($"Groups.{GetGroupFromKey(metricKey)}");
    string itemName = LanguageManager.T($"Items.{metricKey}");
    
    label.Text = $"{groupName} - {itemName}";
    
    // 订阅语言切换
    LanguageManager.OnLanguageChanged += () => {
        string newGroupName = LanguageManager.T($"Groups.{GetGroupFromKey(metricKey)}");
        string newItemName = LanguageManager.T($"Items.{metricKey}");
        label.Text = $"{newGroupName} - {newItemName}";
    };
}
```

## 相关文件

- `src/Core/LanguageManager.cs` - 翻译管理器核心实现
- `resources/lang/*.json` - 翻译文件
- `src/UI/SettingsForm.cs` - 语言切换界面

## 更新记录

| 日期 | 版本 | 更新内容 |
|-----|------|---------|
| 2026-01-30 | 1.0 | 初始文档 |

## 贡献指南

如果您想为 LiteMonitor 添加新的语言支持：

1. Fork 项目仓库
2. 创建新的翻译文件
3. 参考 `en.json` 完成所有翻译
4. 提交 Pull Request
5. 在 PR 描述中说明翻译完成度和测试情况

感谢您的贡献！
