# 原仓库更新整合指南

## 📋 重要更新概览

原仓库（Diorser/LiteMonitor）自你 Fork 后有多个重要更新，建议整合：

### 🔴 高优先级 - Bug 修复

1. **修复主板传感器高频更新问题** (43ffd95)
   - 解决 ForceAll 模式下的性能问题
   - 文件：`src/System/HardwareMonitor.cs`

2. **优化 HttpClient 连接池** (8adec25)
   - 修复资源泄漏问题
   - 文件：`src/Plugins/PluginExecutor.cs`

3. **修复内存负载计算** (1039910)
   - 修正传感器匹配逻辑
   - 文件：`src/System/HardwareServices/SensorMatcher.cs`

4. **修复 GPU 显存匹配** (52a277f)
   - 改进传感器匹配逻辑
   - 文件：`src/System/HardwareServices/SensorMatcher.cs`

### 🟡 中优先级 - 功能改进

5. **重构硬件传感器映射架构** (7fae739)
   - 新增 `HardwareScanner.cs` - 硬件扫描器
   - 新增 `ComponentProcessor.cs` - 组件处理器
   - 新增 `SensorMatcher.cs` - 传感器匹配器
   - 重构 `HardwareValueProvider.cs`
   - 新增 `BatteryService.cs` - 电池服务

6. **重构任务栏策略模式** (daee865)
   - 新增 `TaskbarStrategyWin10.cs` - Win10 策略
   - 新增 `TaskbarStrategyWin11.cs` - Win11 策略
   - 支持任务栏原生挤占模式 (7d557be)

7. **新增任务栏提示帮助** (762375d)
   - 新增 `TaskbarTooltipHelper.cs`
   - 新增 `LiteTooltipForm.cs`

### 🟢 低优先级 - 其他改进

8. **Web 服务器重构**
   - 新增 `WebSessionManager.cs`
   - 优化 `LiteWebServer.cs`

9. **插件系统改进**
   - 新增 `CryptoNative.cs` - 加密货币原生支持
   - 新增 `CityCodeResolver.cs` - 城市代码解析
   - 改进 `PluginExecutor.cs`

10. **多语言更新**
    - 更新所有语言文件
    - 改进翻译准确性

## 🔄 整合建议

### 方案 1：重新基于原仓库（推荐）

如果你想获得最稳定的结果，建议：

```bash
# 1. 备份当前改进
zip -r backup-hotkey-feature.zip src/Core/ErrorHandling src/System/MemoryCleaner docs/

# 2. 重置到原仓库最新版本
git fetch origin
git reset --hard origin/master

# 3. 手动重新应用你的改进
# - 添加 ErrorHandling 目录
# - 添加 MemoryCleaner.cs
# - 添加翻译文档
# - 更新 CleanMemoryForm.cs 使用新的内存清理器
```

### 方案 2：选择性合并

只合并关键的 Bug 修复：

```bash
# 1. 逐个 cherry-pick 关键提交
git cherry-pick 43ffd95  # 主板传感器修复
git cherry-pick 8adec25  # HttpClient 优化
git cherry-pick 1039910  # 内存负载修复
git cherry-pick 52a277f  # GPU 显存修复
```

### 方案 3：手动整合关键文件

仅手动整合最重要的文件：

#### 必须整合的文件：
1. `src/System/HardwareServices/SensorMatcher.cs` - 新建文件
2. `src/System/HardwareServices/HardwareScanner.cs` - 新建文件
3. `src/Plugins/PluginExecutor.cs` - 冲突解决
4. `src/UI/Helpers/TaskbarStrategyWin10.cs` - 新建文件
5. `src/UI/Helpers/TaskbarStrategyWin11.cs` - 新建文件

#### 需要更新的文件：
- `src/System/HardwareMonitor.cs` - 整合传感器高频更新修复
- `src/Core/MetricItem.cs` - 整合内存计算修复
- `src/UI/CleanMemoryForm.cs` - 保留你的改进，整合其他修复

## ⚠️ 注意事项

### HotkeyManager 已删除
原仓库在更新中删除了 `HotkeyManager.cs`，改为其他实现方式。
**你的改进**：统一异常处理在 `HotkeyManager.cs` 中的使用可能需要重新应用。

### 版本号
原仓库最新版本：**v1.3.3**
你 Fork 时的版本：**v1.3.0**

### CleanMemoryForm 变化
原仓库更新了 `CleanMemoryForm.cs`，你需要确保：
1. 保留你的并行清理改进
2. 整合原仓库的 UI 改进
3. 使用新的 `MemoryCleanResult` 结果显示

## 📊 更新对比

| 组件 | 原仓库 | 你的 Fork | 建议 |
|-----|-------|----------|------|
| 硬件监控 | ✅ 重构后 | ❌ 旧版本 | 必须更新 |
| 内存清理 | ✅ 基础功能 | ✅ 并行优化 | 保留你的优化 |
| 异常处理 | ❌ 分散 | ✅ 统一中间件 | 保留你的改进 |
| 任务栏 | ✅ 策略模式 | ❌ 旧版本 | 建议更新 |
| 翻译系统 | ✅ 多语言更新 | ✅ 文档完善 | 两者都保留 |

## 🎯 推荐行动

### 短期（立即）
1. 手动修复关键 Bug：
   - 主板传感器高频更新问题
   - HttpClient 资源泄漏
   - 内存负载计算错误

### 中期（1-2 周）
2. 重构硬件监控模块：
   - 采用新的 `SensorMatcher`
   - 使用 `HardwareScanner`
   - 整合 `ComponentProcessor`

### 长期（可选）
3. 任务栏策略模式：
   - 实现 `TaskbarStrategyWin10`
   - 实现 `TaskbarStrategyWin11`
   - 支持原生挤占模式

## 📝 手动整合步骤

如果你选择手动整合，按以下顺序：

```bash
# 步骤 1: 创建备份分支
git branch backup-hotkey-feature-before-merge

# 步骤 2: 合并原仓库
git merge origin/master

# 步骤 3: 解决冲突（按优先级）
# 高优先级：
# - src/System/HardwareMonitor.cs
# - src/Plugins/PluginExecutor.cs
# - src/Core/MetricItem.cs

# 中优先级：
# - src/UI/CleanMemoryForm.cs（保留你的改进）
# - src/UI/MainForm_Transparent.cs

# 低优先级：
# - README.md（保留你的改进说明）
# - LiteMonitor.csproj

# 步骤 4: 测试编译
dotnet build --configuration Debug

# 步骤 5: 提交合并结果
git add .
git commit -m "merge: 整合原仓库 v1.3.3 更新

整合的改进：
- 修复主板传感器高频更新问题
- 优化 HttpClient 连接池，修复资源泄漏
- 修复内存负载计算和传感器匹配
- 改进 GPU 显存传感器匹配逻辑
- 重构硬件传感器映射架构
- 重构任务栏为策略模式
- 支持 Win10 任务栏原生挤占模式

保留的改进：
- 统一异常处理中间件
- 优化的内存清理器（并行处理）
- 翻译系统文档
- 代码质量提升（空值警告修复）"
```

## 💡 建议

鉴于更改量较大（90+ 文件，6000+ 行变化），**强烈建议**：

1. **重新基于原仓库最新版本**（v1.3.3）
2. **手动重新应用你的核心改进**：
   - ErrorHandling 中间件
   - MemoryCleaner 优化
   - 翻译文档

这样可以确保获得原仓库的所有 Bug 修复，同时保留你的重要改进。

---

需要我帮你执行整合吗？我可以：
1. 创建整合分支
2. 逐步解决冲突
3. 测试编译结果
