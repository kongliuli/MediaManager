# MediaManager 测试计划文档

**版本**: 1.0.0  
**日期**: 2026-04-17  
**项目**: MediaManager — 媒体文件管理系统  

---

## 一、测试范围

基于项目架构（.NET 8.0 / WPF，四层架构），本次测试覆盖以下模块：

| 模块 | 测试类型 | 优先级 |
|------|----------|--------|
| Core 领域模型 | 单元测试 | P0 |
| HashService | 单元测试 | P0 |
| FileScannerService | 单元测试 | P0 |
| DuplicateDetectionService | 单元测试 | P0 |
| SearchService | 单元测试 | P1 |
| MediaRepository | 集成测试 | P1 |
| FfprobeMetadataService | 单元测试 | P1 |
| ScanProgressReport 计算属性 | 单元测试 | P0 |

---

## 二、测试用例清单

### TC-001 ~ TC-010：Core 领域模型

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-001 | DuplicateGroup.ReclaimableSize — 无重复 | Files.Count = 1, FileSize = 1000 | 0 |
| TC-002 | DuplicateGroup.ReclaimableSize — 2个重复 | Files.Count = 2, FileSize = 1000 | 1000 |
| TC-003 | DuplicateGroup.ReclaimableSize — 3个重复 | Files.Count = 3, FileSize = 500 | 1000 |
| TC-004 | DuplicateGroup.ReclaimableSize — 空列表 | Files.Count = 0 | 0 |
| TC-005 | VideoFile.Resolution — 正常分辨率 | Width=1920, Height=1080 | "1920×1080" |
| TC-006 | VideoFile.Resolution — 零值 | Width=0, Height=0 | "0×0" |
| TC-007 | ScanProgressReport.ProgressPercent — 正常进度 | Total=100, Processed=50 | 50.0 |
| TC-008 | ScanProgressReport.ProgressPercent — 未开始 | Total=0, Processed=0 | 0.0 |
| TC-009 | ScanProgressReport.ProgressPercent — 完成 | Total=10, Processed=10 | 100.0 |
| TC-010 | ScanProgressReport.ProgressPercent — 超出不溢出 | Total=10, Processed=15 | 100.0 |

### TC-011 ~ TC-020：FileScannerService

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-011 | IsSupportedFormat — 支持的音频格式 | ".mp3" | true |
| TC-012 | IsSupportedFormat — 支持的视频格式 | ".mp4" | true |
| TC-013 | IsSupportedFormat — 不支持的格式 | ".txt" | false |
| TC-014 | IsSupportedFormat — 大写扩展名 | ".MP3" | true（大小写不敏感）|
| TC-015 | IsSupportedFormat — 所有音频格式 | .wav/.flac/.aac/.ogg/.wma/.m4a/.opus | 全部 true |
| TC-016 | IsSupportedFormat — 所有视频格式 | .avi/.mov/.wmv/.flv/.mkv/.webm/.m4v | 全部 true |
| TC-017 | DiscoverFilesAsync — 目录不存在 | 不存在的路径 | 返回空序列（不抛异常）|
| TC-018 | DiscoverFilesAsync — 空目录 | 空目录 | 返回空序列 |
| TC-019 | DiscoverFilesAsync — 含媒体文件目录 | 含 .mp3/.mp4 文件的目录 | 返回这些文件路径 |
| TC-020 | DiscoverFilesAsync — 取消令牌 | 立即取消的 CancellationToken | 抛出 OperationCanceledException |

### TC-021 ~ TC-030：DuplicateDetectionService

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-021 | FindDuplicatesAsync — 无重复 | 所有文件哈希唯一 | 返回空列表 |
| TC-022 | FindDuplicatesAsync — 有一组重复 | 2个文件同哈希 | 返回1个 DuplicateGroup |
| TC-023 | FindDuplicatesAsync — 多组重复 | 3组各2个重复 | 返回3个 DuplicateGroup |
| TC-024 | FindDuplicatesAsync — 文件不存在于仓储 | GetByIdAsync 返回 null | 该文件被跳过，组内不足2个则不加入结果 |
| TC-025 | FindDuplicatesAsync — 空仓储 | GetAllHashesAsync 返回空 | 返回空列表 |

### TC-031 ~ TC-040：MediaRepository（集成测试，使用 SQLite InMemory）

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-031 | UpsertAsync — 插入新文件 | 新 AudioFile | 数据库中存在该记录 |
| TC-032 | UpsertAsync — 更新已有文件 | 相同 Path 的文件，修改 Rating | 数据库中 Rating 已更新 |
| TC-033 | GetByIdAsync — 存在的 ID | 已插入文件的 ID | 返回对应文件 |
| TC-034 | GetByIdAsync — 不存在的 ID | 999999 | 返回 null |
| TC-035 | GetByPathAsync — 存在的路径 | 已插入文件的 Path | 返回对应文件 |
| TC-036 | DeleteAsync — 存在的文件 | 已插入文件的 ID | 数据库中不再存在 |
| TC-037 | SearchAsync — 关键词过滤 | Keyword="test" | 只返回 FileName 含 "test" 的文件 |
| TC-038 | SearchAsync — 分页 | Page=2, PageSize=2, 共5条 | 返回第3、4条 |
| TC-039 | SearchAsync — 按时长排序 | SortBy=Duration, SortDescending=true | 按时长降序排列 |
| TC-040 | GetAllHashesAsync — 含空哈希 | 部分文件 Hash 为空 | 只返回非空哈希的文件 |

### TC-R01 ~ TC-R05：真实目录集成测试（FileScannerService）

测试目录：`C:\Users\admin.DESKTOP-4R9CSJI\Documents\xwechat_files\wxid_qbctxev48abj21_23fa\msg\file`  
目录结构：按月份分层（2025-10, 2025-11, 2025-12, 2026-01, 2026-02, 2026-03 ...），递归深度 2 层

| ID | 测试项 | 预期结果 |
|----|--------|----------|
| TC-R01 | 递归扫描真实目录 | 返回非空媒体文件列表 |
| TC-R02 | 递归扫描 — 发现 .mp3 文件 | 至少找到1个 .mp3 文件 |
| TC-R03 | 递归扫描 — 发现 .mp4/.mov 文件 | 至少找到1个视频文件 |
| TC-R04 | 非递归扫描根目录 | 返回空（文件都在子目录中）|
| TC-R05 | 递归扫描 — 过滤非媒体文件 | 不包含 .pdf/.jpg/.png/.docx |

---

## 三、测试技术栈

- **测试框架**: xUnit 2.9
- **Mock 框架**: Moq 4.20
- **断言库**: FluentAssertions 6.12
- **数据库**: Microsoft.EntityFrameworkCore.InMemory（集成测试）
- **目标框架**: net8.0

---

## 四、测试项目结构

```
tests/
└── MediaManager.Tests/
    ├── MediaManager.Tests.csproj
    ├── Core/
    │   ├── DuplicateGroupTests.cs
    │   ├── VideoFileTests.cs
    │   └── ScanProgressReportTests.cs
    ├── Services/
    │   ├── FileScannerServiceTests.cs
    │   ├── DuplicateDetectionServiceTests.cs
    │   └── HashServiceTests.cs
    └── Data/
        └── MediaRepositoryTests.cs
```
