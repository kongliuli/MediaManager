# Tasks - UI数据刷新问题修复

## 任务 1: 创建扫描完成消息类
- [x] Task 1.1: 在 `MediaManager.Core/Messages/ScanCompletedMessage.cs` 中添加 `ScanCompletedMessage` 类

## 任务 2: 修改 ScanPipelineOrchestrator 发送扫描完成消息
- [x] Task 2.1: 在 `ScanPipelineOrchestrator.cs` 中引入 `WeakReferenceMessenger`
- [x] Task 2.2: 在扫描完成的 finally 块中发送 `ScanCompletedMessage`

## 任务 3: 修改 LibraryViewModel 订阅扫描完成消息
- [x] Task 3.1: 让 `LibraryViewModel` 实现 `IRecipient<ScanCompletedMessage>` 接口
- [x] Task 3.2: 实现 `Receive(ScanCompletedMessage message)` 方法，调用 `LoadAsync()`
- [x] Task 3.3: 在构造函数中注册消息订阅

## 任务 4: 修改 MediaLibraryViewModel 订阅扫描完成消息
- [x] Task 4.1: 让 `MediaLibraryViewModel` 实现 `IRecipient<ScanCompletedMessage>` 接口
- [x] Task 4.2: 实现 `Receive(ScanCompletedMessage message)` 方法，调用 `LoadLibrariesAsync()`
- [x] Task 4.3: 在构造函数中注册消息订阅

## 任务 5: 修复 EF Core 数据跟踪问题
- [x] Task 5.1: 检查 `MediaRepository.SearchAsync` 方法 - 已添加 `AsNoTracking()`
- [x] Task 5.2: 在 `GetAllHashesAsync` 也添加 `AsNoTracking()`

## 任务 6: 修复 BoolToVisibilityConverter Invert 属性
- [x] Task 6.1: 检查 `BoolToVisibilityConverter.cs` 实现
- [x] Task 6.2: 修复 `ConverterParameter=Invert` 的处理逻辑

## 任务 7: 验证和测试
- [x] Task 7.1: 所有项目构建成功
- [x] Task 7.2: 解决方案已通过编译验证