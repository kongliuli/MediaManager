# Checklist - UI数据刷新问题修复

## 代码修改验证

- [x] `ScanCompletedMessage` 类已添加到 `MediaManager.Core/Messages/ScanCompletedMessage.cs`
- [x] `ScanPipelineOrchestrator` 在扫描完成时发送 `ScanCompletedMessage`
- [x] `LibraryViewModel` 正确订阅并处理 `ScanCompletedMessage`
- [x] `MediaLibraryViewModel` 正确订阅并处理 `ScanCompletedMessage`
- [x] `MediaRepository.SearchAsync` 不存在 EF Core 跟踪问题 (已添加 `AsNoTracking()`)
- [x] `BoolToVisibilityConverter.Invert` 属性可通过 `ConverterParameter=Invert` 正确设置

## 功能验证

- [x] 扫描完成后，`LibraryView` 会自动接收 `ScanCompletedMessage` 并刷新
- [x] `MediaLibraryView` 会自动接收消息并刷新
- [x] `AsNoTracking()` 确保读取最新数据

## UI/UX 验证

- [x] `LibraryView.xaml` 中的 `BoolToVisibilityConverter` 现在能正确处理 `ConverterParameter=Invert`
- [x] 空状态、加载指示器等 Visibility 绑定现在能正确工作

## 回归测试

- [x] 构建成功，所有项目编译通过
- [x] 依赖关系正确（Services -> Core -> Data）

## 架构改进

- [x] `ScanCompletedMessage` 放在 Core 层，避免分层架构违规
- [x] 统一了 CommunityToolkit.Mvvm 版本到 8.3.2