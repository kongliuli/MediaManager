# MediaManager - 实施计划（分解和优先级任务列表）

## [ ] 任务 1: 创建文件路径验证工具类
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 在 MediaManager.Core 项目中创建路径验证工具类
  - 实现防止路径遍历攻击的验证方法
  - 实现路径规范化和安全检查功能
- **Acceptance Criteria Addressed**: [AC-4]
- **Test Requirements**:
  - `programmatic` TR-1.1: 验证包含 "../" 的路径被正确拒绝
  - `programmatic` TR-1.2: 验证绝对路径在允许目录范围内
  - `programmatic` TR-1.3: 验证路径规范化功能正常工作
- **Notes**: 工具类应命名为 PathValidator 或类似名称，放在 Helpers 或 Utilities 目录下。

## [ ] 任务 2: 实现统一错误处理机制
- **Priority**: P0
- **Depends On**: 任务 1
- **Description**: 
  - 在 MediaManager.Core 项目中创建自定义异常类
  - 实现错误处理中间件或服务
  - 为服务层添加异常处理包装
  - 确保错误信息不泄露敏感信息
- **Acceptance Criteria Addressed**: [AC-1]
- **Test Requirements**:
  - `programmatic` TR-2.1: 验证异常被正确捕获和记录
  - `programmatic` TR-2.2: 验证错误信息不包含敏感路径信息
  - `human-judgement` TR-2.3: 检查代码中的异常处理是否一致
- **Notes**: 使用 Serilog 进行日志记录，自定义异常应继承自 Exception 基类。

## [ ] 任务 3: 实现进度报告机制
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 在 MediaManager.Core 项目中创建进度报告 DTO
  - 定义进度报告事件或回调接口
  - 更新服务接口以支持进度报告
- **Acceptance Criteria Addressed**: [AC-3]
- **Test Requirements**:
  - `programmatic` TR-3.1: 验证进度报告对象包含所有必要字段
  - `programmatic` TR-3.2: 验证进度百分比计算正确
- **Notes**: 进度报告应包含：当前文件、已处理数量、总数、百分比、估计剩余时间。

## [ ] 任务 4: 更新文件扫描服务以支持增量扫描和进度报告
- **Priority**: P0
- **Depends On**: 任务 1, 任务 3
- **Description**: 
  - 修改 IFileScannerService 接口以支持进度报告
  - 更新 FileScannerService 实现
  - 添加增量扫描逻辑，比较文件的最后修改时间和大小
  - 集成路径验证
- **Acceptance Criteria Addressed**: [AC-2, AC-3, AC-4]
- **Test Requirements**:
  - `programmatic` TR-4.1: 验证增量扫描只处理变化的文件
  - `programmatic` TR-4.2: 验证进度报告事件被正确触发
  - `programmatic` TR-4.3: 验证路径验证被正确应用
- **Notes**: 增量扫描需要从数据库获取已有文件信息进行比较。

## [ ] 任务 5: 创建测试项目和基础结构
- **Priority**: P1
- **Depends On**: None
- **Description**: 
  - 创建 MediaManager.Tests 测试项目
  - 添加必要的测试依赖（xUnit, Moq, FluentAssertions 等）
  - 配置测试项目的依赖注入
  - 创建测试基类和辅助方法
- **Acceptance Criteria Addressed**: [AC-5]
- **Test Requirements**:
  - `programmatic` TR-5.1: 验证测试项目可以正常编译
  - `programmatic` TR-5.2: 验证测试基础设施可以正常工作
- **Notes**: 使用 xUnit 作为测试框架，Moq 用于模拟依赖。

## [ ] 任务 6: 为核心服务编写单元测试
- **Priority**: P1
- **Depends On**: 任务 5
- **Description**: 
  - 为 PathValidator 编写单元测试
  - 为 HashService 编写单元测试
  - 为 FileScannerService 编写单元测试
  - 为其他核心服务添加基本测试
- **Acceptance Criteria Addressed**: [AC-5]
- **Test Requirements**:
  - `programmatic` TR-6.1: 验证所有测试可以正常运行
  - `programmatic` TR-6.2: 验证核心服务的测试覆盖率达到 50% 以上
- **Notes**: 测试应覆盖正常场景、边界情况和错误场景。
