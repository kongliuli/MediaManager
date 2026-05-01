# 媒体类型扩充系统 - Product Requirement Document

## Overview
- **Summary**: 构建一个可扩展的媒体类型分类系统，支持细分类别、插件式组件处理、灵活的识别配置。
- **Purpose**: 解决当前系统媒体类型过于简单（仅4种）的问题，提供更智能的分类和更专业的处理功能。
- **Target Users**: 媒体库管理员、开发者、终端用户

## Goals
- **Goal 1**: 实现完整的媒体类型层次结构（主类型 + 子类型）
- **Goal 2**: 建立未知类型/子类型的识别和处理机制
- **Goal 3**: 实现类型识别开关配置系统
- **Goal 4**: 构建插件式组件系统，支持针对特定类型的处理
- **Goal 5**: 设计主页面契约，统一媒体操作接口
- **Goal 6**: 集成开源库（如 ML、格式转换、音轨提取等）

## Non-Goals (Out of Scope)
- **Non-Goal 1**: 重写现有数据结构（兼容现有数据库）
- **Non-Goal 2**: 重写完整的 UI 界面（渐进式更新）
- **Non-Goal 3**: 实现所有 AI 功能（仅构建基础架构）

## Background & Context
- 现有系统支持4种基础类型：Audio, Video, Image, Other
- 当前无细分类别，无法针对特定类型提供专业处理
- 无插件系统，难以扩展新功能
- 参考了 [深化架构建议.md](file:///workspace/深化架构建议.md) 的内容

## Functional Requirements

### **FR-1**: 媒体类型层次结构
- 支持主类型（Audio, Video, Image, Document）
- 每个主类型下支持多个子类型（共30+种）
- 每个类型有特定的元数据结构
- 支持自定义类型扩展

### **FR-2**: 未知类型处理
- 未识别的媒体放入"未知池"
- 子类型未识别的保留在基类下
- 支持手动分类和学习

### **FR-3**: 识别配置系统
- 每个类型/子类型可独立开关识别
- 全局识别开关
- 支持自动降级（高级识别失败时使用基础识别）

### **FR-4**: 插件组件系统
- 组件具有标签属性（支持的类型）
- 组件有输入类型和输出类型
- 支持 DLL 插件加载
- 组件可按需激活
- 组件有依赖关系管理

### **FR-5**: 开源库集成
- 在设置页配置启用/禁用集成
- 支持 Whisper（音频转文字）
- 支持格式转换组件
- 支持音轨提取组件
- 支持更多扩展组件

### **FR-6**: 主页面契约
- 统一媒体操作接口
- 基于媒体类型自动显示可用操作
- 操作链支持

## Non-Functional Requirements
- **NFR-1**: 向后兼容现有数据库结构
- **NFR-2**: 插件加载不超过1秒
- **NFR-3**: 类型识别准确率 >90%
- **NFR-4**: 配置更改立即生效（无需重启）

## Constraints
- **Technical**: 基于 .NET 8.0, Blazor, EF Core
- **Business**: 保持现有功能完整
- **Dependencies**: 现有 MediaManager.Core, Data, Services 项目

## Assumptions
- **Assumption 1**: 现有数据库可以通过新增字段支持新架构
- **Assumption 2**: 用户愿意逐步迁移到新分类
- **Assumption 3**: 插件将遵循标准接口

## Acceptance Criteria

### **AC-1**: 媒体类型层次结构
- **Given**: 系统已部署
- **When**: 查看类型枚举
- **Then**: 可以看到完整的类型层次（主类型 + 子类型）
- **Verification**: `programmatic`

### **AC-2**: 未知类型处理
- **Given**: 有一个无法识别的媒体文件
- **When**: 系统扫描该文件
- **Then**: 文件被标记为"未知"，放入未知池
- **Verification**: `programmatic`

### **AC-3**: 识别配置
- **Given**: 用户在设置页面
- **When**: 关闭某个类型的识别
- **Then**: 该类型的媒体不再被识别（放入未知池）
- **Verification**: `programmatic` + `human-judgment`

### **AC-4**: 插件组件加载
- **Given**: 插件 DLL 存在于指定目录
- **When**: 系统启动
- **Then**: 插件被自动加载，类型组件可用
- **Verification**: `programmatic`

### **AC-5**: 组件标签属性
- **Given**: 有一个音频处理组件
- **When**: 查看组件标签
- **Then**: 显示该组件支持的媒体类型
- **Verification**: `programmatic`

### **AC-6**: 开源库集成配置
- **Given**: 用户在设置页面
- **When**: 启用 Whisper 集成
- **Then**: 音频文件显示"转文字"操作
- **Verification**: `programmatic` + `human-judgment`

### **AC-7**: 主页面契约
- **Given**: 用户在媒体详情页
- **When**: 媒体是音频类型
- **Then**: 显示音频相关的操作按钮
- **Verification**: `human-judgment`

### **AC-8**: 格式转换组件
- **Given**: 用户选择音频文件
- **When**: 点击"转换格式"
- **Then**: 可以选择输出格式并转换
- **Verification**: `human-judgment`

### **AC-9**: 音轨提取组件
- **Given**: 用户选择视频文件
- **When**: 点击"提取音轨"
- **Then**: 视频中的音频被提取为单独文件
- **Verification**: `human-judgment`

## Open Questions
- [ ] 未知类型池应该在内存中还是持久化到数据库？
- [ ] 组件系统是否需要沙箱安全机制？
- [ ] 用户自定义类型的优先级如何处理？
