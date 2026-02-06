# LiteMonitor 异常处理中间件和快捷键功能改进

## 概述

本项目在 [Diorser/LiteMonitor](https://bgithub.xyz/Diorser/LiteMonitor) 的基础上，实现了统一的异常处理中间件，并修复了快捷键设置页面的翻译问题。

## 主要改进

### 1. 统一异常处理中间件 (ErrorHandling)

#### 1.1 架构设计
创建了完整的异常处理架构，位于 `src/Core/ErrorHandling/` 目录：

- **ErrorSeverity.cs** - 错误严重级别枚举
  - `Info` - 信息级别，用于调试和跟踪
  - `Warning` - 警告级别，非致命问题，需要关注
  - `Error` - 错误级别，功能受影响但程序可继续运行
  - `Critical` - 严重级别，程序可能崩溃或无法继续运行

- **IErrorHandler.cs** - 错误处理接口
  - `HandleException()` - 处理异常并记录日志
  - `HandleError()` - 处理错误消息（无异常对象）
  - `TryExecute()` - 尝试执行操作，自动处理异常
  - `ShowUserNotification()` - 显示用户友好的错误提示
  - `LogError()` - 记录错误日志

- **ErrorHandler.cs** - 错误处理实现类
  - 单例模式，全局唯一实例
  - 统一的错误日志格式（包含时间、级别、上下文、堆栈）
  - 集成现有 Logger 类，保持向后兼容
  - 支持配置选项（用户通知、文件日志）

#### 1.2 核心功能

**统一错误处理**
```csharp
// 处理异常
ErrorHandler.Instance.HandleException(ex, "Context", ErrorSeverity.Error);

// 处理错误消息
ErrorHandler.Instance.HandleError("错误消息", "Context", ErrorSeverity.Warning);
```

**TryExecute 模式**
```csharp
// 简化异常处理
bool success = ErrorHandler.Instance.TryExecute(() => {
    // 可能抛出异常的代码
}, "操作上下文");

// 带返回值
var result = ErrorHandler.Instance.TryExecute(() => {
    return SomeOperation();
}, "上下文", defaultValue);
```

**向后兼容**
```csharp
// 继续使用现有 Logger
ErrorHandler.Log.Info("信息消息");
ErrorHandler.Log.Error("错误消息", exception);
```

#### 1.3 集成位置

- **Program.cs** - 全局异常捕获
  - UI 线程异常处理
  - 后台线程异常处理
  
- **HotkeyManager.cs** - 热键相关异常
  - 初始化异常
  - 热键注册/注销异常
  - 配置重新加载异常

### 2. HotkeyPage.cs 修复

#### 2.1 修复的问题

**命名空间冲突**
- 修复了 `ModifierKeys` 与 `System.Windows.Forms.Keys` 的冲突
- 使用 `HotkeyManager.ModifierKeys` 明确指定

**翻译显示**
- 确保所有文本使用 `LanguageManager.T()` 正确显示中文
- 添加了健壮的异常处理，防止 UI 崩溃

#### 2.2 改进的健壮性
- 添加了完整的 try-catch 块
- 使用统一错误处理中间件
- 提供用户友好的错误提示

### 3. 代码质量改进

#### 3.1 标准化
- 所有异常处理使用统一接口
- 统一的错误日志格式
- 一致的错误严重级别分类

#### 3.2 可维护性
- 错误处理逻辑集中管理
- 便于修改和扩展
- 清晰的代码结构

#### 3.3 用户体验
- 提供适当的错误提示
- 避免程序静默崩溃
- 错误日志易于追踪

## 编译状态

```bash
✅ 编译成功
- 0 个错误
- 203 个警告（都是原有的空值警告，与异常处理无关）
```

## 文件变更

### 新增文件
- `src/Core/ErrorHandling/ErrorSeverity.cs`
- `src/Core/ErrorHandling/IErrorHandler.cs`
- `src/Core/ErrorHandling/ErrorHandler.cs`

### 修改文件
- `src/System/Program.cs` - 使用 ErrorHandler 替换 LogCrash
- `src/System/HotkeyManager.cs` - 更新所有 catch 块
- `src/UI/Settings/HotkeyPage.cs` - 修复翻译问题和异常处理
- `src/Core/Actions/AppActions.cs`
- `src/Core/Settings.cs`
- `src/Plugins/PluginExecutor.cs`
- `src/Plugins/PluginManager.cs`
- `src/UI/Controls/LiteUI.cs`
- `src/UI/HardwareInfoForm.cs`
- `src/UI/Helpers/MainFormBizHelper.cs`
- `src/UI/MainForm_Transparent.cs`
- `resources/lang/zh.json`
- `LiteMonitor.csproj`

### 删除文件
- `src/UI/TaskbarForm.cs.bak`
- `快捷键功能实现总结.md`
- `快捷键功能实现方案.md`

## 技术亮点

### 1. 设计模式
- **单例模式**：ErrorHandler 使用单例确保全局唯一实例
- **策略模式**：不同的错误严重级别对应不同的处理策略
- **外观模式**：简化异常处理接口，隐藏复杂逻辑

### 2. 向后兼容
- 保留现有 Logger 类
- 提供便捷方法 `ErrorHandler.Log`
- 兼容现有的日志文件格式

### 3. 配置灵活性
- 可启用/禁用用户通知
- 可启用/禁用文件日志
- 可自定义错误日志路径

## 使用示例

### 基本异常处理
```csharp
try
{
    // 可能抛出异常的代码
}
catch (Exception ex)
{
    ErrorHandler.Instance.HandleException(ex, "操作上下文");
}
```

### TryExecute 模式
```csharp
// 无需 try-catch
bool success = ErrorHandler.Instance.TryExecute(() => {
    RiskyOperation();
}, "RiskyOperation");

if (!success)
{
    // 处理失败情况
}
```

### 带返回值的 TryExecute
```csharp
var data = ErrorHandler.Instance.TryExecute(() => {
    return LoadData();
}, "LoadData", defaultValue: null);

if (data != null)
{
    // 使用数据
}
```

### 自定义错误处理
```csharp
ErrorHandler.Instance.TryExecute(() => {
    RiskyOperation();
}, "Context", onError: (ex) => {
    // 自定义错误处理逻辑
    Logger.Warn($"操作失败: {ex.Message}");
});
```

## 测试建议

1. **异常场景测试**
   - UI 线程异常
   - 后台线程异常
   - 文件 I/O 异常
   - 网络异常

2. **边界条件测试**
   - 磁盘满的情况
   - 权限受限的情况
   - 空引用异常

3. **用户体验测试**
   - 错误提示是否友好
   - 日志文件是否正确生成
   - 程序是否稳定运行

## 注意事项

1. **不合并到主分支**：此分支为功能改进分支，保持独立
2. **保持向后兼容**：现有代码可以继续使用 Logger 类
3. **空值警告**：项目原有 203 个空值警告，建议后续逐步修复

## 贡献者

- **xuha233** - 实现统一异常处理中间件和快捷键功能修复

## 许可证

与原项目保持一致

## 相关链接

- 原项目：[https://bgithub.xyz/Diorser/LiteMonitor](https://bgithub.xyz/Diorser/LiteMonitor)
- Fork 仓库：[https://github.com/xuha233/LiteMonitor](https://github.com/xuha233/LiteMonitor)
