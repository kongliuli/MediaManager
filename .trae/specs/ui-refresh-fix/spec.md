# UI数据刷新问题修复 - 产品需求文档

## Why
用户报告：数据已成功扫描并存入数据库，但UI无法刷新显示新增的媒体文件。这是一个关键的可用性问题，影响用户对扫描结果的验证。

根据代码审查，识别出以下根本原因：

1. **缺少扫描完成通知机制**：扫描管道完成后不会通知相关 ViewModel 刷新数据
2. **EF Core 跟踪问题**：`MediaDbContext` 可能对同一上下文中的实体进行了跟踪，导致读取到缓存的旧数据
3. **XAML 转换器 bug**：`BoolToVisibilityConverter.Invert` 属性无法通过 `ConverterParameter` 设置

## What Changes

### 核心修复

- **[BUG-FIX-1]** 添加扫描完成消息通知机制
  - 创建 `ScanCompletedMessage` 消息类
  - 在 `ScanPipelineOrchestrator` 扫描完成时发送该消息
  - `LibraryViewModel` 订阅该消息并在收到后自动刷新数据

- **[BUG-FIX-2]** 修复 EF Core 数据跟踪问题
  - 确保 `MediaRepository` 在读取数据时使用 `AsNoTracking()`
  - 或在每次查询前调用 `ChangeTracker.Clear()`

- **[BUG-FIX-3]** 修复 XAML 绑定中的 `Invert` 属性问题
  - 检查 `BoolToVisibilityConverter` 的实现
  - 确保 XAML 中的 `ConverterParameter=Invert` 能正确工作

### 增强功能

- **[ENHANCE-1]** 添加手动刷新命令
  - 在 `LibraryViewModel` 和 `MediaLibraryViewModel` 中确保 `LoadCommand` 能正确触发刷新
  - UI 绑定到 `LoadCommand` 而非直接调用 `LoadAsync()`

## Impact

### 受影响的规格
- AC-3 (进度反馈机制) - 需确保进度更新不冻结 UI

### 受影响的代码
- `MediaManager.Services/Scanning/ScanPipelineOrchestrator.cs` - 添加扫描完成通知
- `MediaManager.UI/ViewModels/LibraryViewModel.cs` - 订阅扫描完成消息
- `MediaManager.UI/ViewModels/MediaLibraryViewModel.cs` - 可能需要订阅消息
- `MediaManager.UI/Converters/BoolToVisibilityConverter.cs` - 修复 Invert 属性
- `MediaManager.Data/Repositories/MediaRepository.cs` - 修复数据跟踪问题

## ADDED Requirements

### Requirement: 扫描完成自动刷新
系统 SHALL 在扫描完成后自动通知所有需要刷新数据的 ViewModel。

#### Scenario: 扫描完成自动刷新
- **GIVEN** 用户完成了一次媒体文件扫描
- **WHEN** 扫描管道执行完毕（成功或失败）
- **THEN** `LibraryViewModel` 自动接收 `ScanCompletedMessage` 并刷新 `Items` 集合
- **AND** `MediaLibraryViewModel` (如果已实例化) 自动刷新 `Libraries` 列表

### Requirement: 数据层正确读取最新数据
系统 SHALL 确保从数据库读取的数据是最新的，不受 EF Core 跟踪缓存影响。

#### Scenario: 读取最新扫描数据
- **GIVEN** 刚刚完成一次扫描，新增了 10 个媒体文件到数据库
- **WHEN** 用户切换到媒体库页面
- **THEN** UI 正确显示所有 10 个新增文件
- **AND** 不需要重启应用程序

## MODIFIED Requirements

### Requirement: BoolToVisibilityConverter Invert 功能
**原需求**：通过 `ConverterParameter=Invert` 应反转布尔值的转换结果
**修改后**：修复实现，确保 `ConverterParameter=Invert` 能正确反转布尔值

#### Scenario: Invert 参数正确工作
- **GIVEN** 一个 `BoolToVisibilityConverter` 实例，且 `Invert=true`
- **WHEN** 绑定值为 `true`
- **THEN** 转换结果为 `Visibility.Collapsed`
- **WHEN** 绑定值为 `false`
- **THEN** 转换结果为 `Visibility.Visible`

## REMOVED Requirements
无

## Technical Details

### 消息机制
使用 `CommunityToolkit.Mvvm.WeakReferenceMessenger` 进行消息传递：
- `ScanCompletedMessage` - 包含扫描结果摘要
- 订阅者：`LibraryViewModel`, `MediaLibraryViewModel`

### EF Core 优化
在 `MediaRepository.SearchAsync` 中添加 `AsNoTracking()` 查询优化：
```csharp
var dbQuery = dbContext.MediaFiles.AsQueryable(); // 考虑添加 AsNoTracking()
```

### 转换器修复
`BoolToVisibilityConverter` 需要检查是否正确实现了 `Invert` 属性绑定。

## Open Questions
- [ ] 是否需要在扫描过程中也实时更新 UI（如发现新文件时）？
- [ ] 是否需要为 `SearchViewModel` 也添加扫描完成刷新？
