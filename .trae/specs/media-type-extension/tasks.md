# 媒体类型扩充系统 - The Implementation Plan (Decomposed and Prioritized Task List)

## [x] Task 1: 定义媒体类型层次结构（Core 层）
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 在 MediaManager.Core 中定义完整的类型枚举
  - 定义主类型（Audio, Video, Image, Document）
  - 定义每个主类型的子类型（30+种）
  - 定义每个类型的元数据结构
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-1.1: 类型枚举完整定义
  - `programmatic` TR-1.2: 每个主类型有对应的子类型
  - `programmatic` TR-1.3: 元数据结构定义完整
- **Notes**: 保持向后兼容，不要删除现有 MediaType 枚举
- **Status**: ✅ Completed

## [x] Task 2: 更新数据模型（Data 层）
- **Priority**: P0
- **Depends On**: Task 1
- **Description**: 
  - 扩展 MediaFile 基类，添加 SubType 属性
  - 为每种类型定义专门的派生类（AudioFile, VideoFile 等）
  - 更新 MediaDbContext 配置
  - 添加迁移或兼容层
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-2.1: MediaFile 有 SubType 属性
  - `programmatic` TR-2.2: EF Core 映射正确
  - `programmatic` TR-2.3: 现有数据库可以正常加载
- **Status**: ✅ Completed

## [ ] Task 3: 实现类型识别器
- **Priority**: P0
- **Depends On**: Task 1, Task 2
- **Description**: 
  - 实现 IMediaTypeRecognizer 接口
  - 实现基于文件扩展名的基础识别
  - 实现基于元数据的增强识别
  - 实现识别失败时的降级策略
- **Acceptance Criteria Addressed**: AC-1, AC-2
- **Test Requirements**:
  - `programmatic` TR-3.1: 基础扩展名识别正确
  - `programmatic` TR-3.2: 元数据识别准确率 >90%
  - `programmatic` TR-3.3: 未知文件被正确标记

## [ ] Task 4: 实现识别配置系统
- **Priority**: P1
- **Depends On**: Task 3
- **Description**: 
  - 定义 TypeRecognitionSettings 配置类
  - 实现每个类型的开关配置
  - 实现配置的持久化（JSON/数据库）
  - 实现配置热重载
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic` TR-4.1: 配置可以保存和加载
  - `programmatic` TR-4.2: 关闭某个类型后该类型不被识别
  - `human-judgement` TR-4.3: 配置更改无需重启生效

## [ ] Task 5: 实现未知类型池
- **Priority**: P1
- **Depends On**: Task 3, Task 4
- **Description**: 
  - 实现 IUnknownMediaPool 接口
  - 实现未知类型的存储和检索
  - 实现手动分类功能
  - 实现从未知池到已知类型的迁移
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `programmatic` TR-5.1: 未知文件被正确放入池
  - `programmatic` TR-5.2: 可以手动分配类型
  - `programmatic` TR-5.3: 可以从池中移除文件

## [x] Task 6: 定义组件系统契约
- **Priority**: P0
- **Depends On**: Task 1
- **Description**: 
  - 定义 IMediaComponent 基接口
  - 定义组件标签属性（MediaComponentAttribute）
  - 定义输入类型、输出类型契约
  - 定义组件生命周期管理
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-6.1: 接口定义完整
  - `programmatic` TR-6.2: 属性定义正确
  - `programmatic` TR-6.3: 生命周期接口完整
- **Status**: ✅ Completed

## [ ] Task 7: 实现插件加载系统
- **Priority**: P1
- **Depends On**: Task 6
- **Description**: 
  - 实现 IPluginLoader 接口
  - 实现 DLL 扫描和加载
  - 实现组件依赖解析
  - 实现插件隔离和安全检查
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-7.1: DLL 可以被正确加载
  - `programmatic` TR-7.2: 组件依赖正确解析
  - `programmatic` TR-7.3: 加载时间 <1秒

## [ ] Task 8: 实现开源库集成接口
- **Priority**: P1
- **Depends On**: Task 6, Task 7
- **Description**: 
  - 定义 IIntegrationSettings 接口
  - 实现 Whisper 集成（音频转文字）
  - 实现格式转换组件
  - 实现音轨提取组件
  - 在设置页提供配置 UI
- **Acceptance Criteria Addressed**: AC-6, AC-8, AC-9
- **Test Requirements**:
  - `programmatic` TR-8.1: 集成开关配置正确
  - `human-judgement` TR-8.2: Whisper 转换功能正常
  - `human-judgement` TR-8.3: 格式转换功能正常
  - `human-judgement` TR-8.4: 音轨提取功能正常

## [ ] Task 9: 设计主页面契约
- **Priority**: P1
- **Depends On**: Task 6, Task 8
- **Description**: 
  - 定义 IMediaOperations 接口
  - 实现基于媒体类型的操作发现
  - 实现操作链执行（多个组件顺序执行）
  - 在 UI 层实现操作按钮的动态显示
- **Acceptance Criteria Addressed**: AC-5, AC-7
- **Test Requirements**:
  - `human-judgement` TR-9.1: 不同类型显示不同操作
  - `programmatic` TR-9.2: 操作发现接口完整
  - `human-judgement` TR-9.3: 操作链可以执行

## [ ] Task 10: 实现设置页面 UI
- **Priority**: P2
- **Depends On**: Task 4, Task 8
- **Description**: 
  - 在 Blazor UI 中创建设置页面
  - 实现类型识别开关
  - 实现集成库配置
  - 实现插件管理界面
- **Acceptance Criteria Addressed**: AC-3, AC-6
- **Test Requirements**:
  - `human-judgement` TR-10.1: 设置页面布局合理
  - `human-judgement` TR-10.2: 开关功能正常
  - `human-judgement` TR-10.3: 插件可以被管理

## [ ] Task 11: 编写集成测试
- **Priority**: P2
- **Depends On**: Task 1-10
- **Description**: 
  - 编写类型识别测试
  - 编写插件加载测试
  - 编写集成库功能测试
  - 编写端到端场景测试
- **Acceptance Criteria Addressed**: AC-1, AC-2, AC-4, AC-6
- **Test Requirements**:
  - `programmatic` TR-11.1: 所有核心功能有测试覆盖
  - `programmatic` TR-11.2: 测试通过率 >95%
  - `programmatic` TR-11.3: 集成测试覆盖主要场景
