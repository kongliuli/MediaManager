# MediaManager 测试文档

**版本**: 1.0.0  
**日期**: 2026-04-17  
**项目**: MediaManager — 媒体文件管理系统（.NET 8.0 / WPF）  
**测试框架**: xUnit 2.8.2 / Moq 4.20 / FluentAssertions 6.12 / EF InMemory  

---

## 一、测试范围

| 模块 | 测试类型 | 优先级 |
|------|----------|--------|
| Core 领域模型 | 单元测试 | P0 |
| ScanProgressReport 计算属性 | 单元测试 | P0 |
| FileScannerService | 单元测试 + 真实目录集成 | P0 |
| DuplicateDetectionService | 单元测试（Mock 仓储） | P0 |
| MediaRepository | 集成测试（EF InMemory） | P1 |
| HashService | 未覆盖（依赖真实文件 I/O） | P1 |
| FfprobeMetadataService | 未覆盖（依赖 ffprobe 进程） | P1 |

---

## 二、测试用例清单

### TC-001 ~ TC-010：Core 领域模型

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-001 | DuplicateGroup.ReclaimableSize — 无重复 | Files.Count=1, FileSize=1000 | 0 |
| TC-002 | DuplicateGroup.ReclaimableSize — 2个重复 | Files.Count=2, FileSize=1000 | 1000 |
| TC-003 | DuplicateGroup.ReclaimableSize — 3个重复 | Files.Count=3, FileSize=500 | 1000 |
| TC-004 | DuplicateGroup.ReclaimableSize — 空列表 | Files.Count=0 | 0 |
| TC-005 | VideoFile.Resolution — 正常分辨率 | Width=1920, Height=1080 | "1920×1080" |
| TC-006 | VideoFile.Resolution — 零值 | Width=0, Height=0 | "0×0" |
| TC-007 | ScanProgressReport.ProgressPercent — 正常进度 | Total=100, Processed=50 | 50.0 |
| TC-008 | ScanProgressReport.ProgressPercent — 未开始 | Total=0, Processed=0 | 0.0 |
| TC-009 | ScanProgressReport.ProgressPercent — 完成 | Total=10, Processed=10 | 100.0 |
| TC-010 | ScanProgressReport.ProgressPercent — 超出不溢出 | Total=10, Processed=15 | 100.0 |

### TC-011 ~ TC-020：FileScannerService

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-011 | IsSupportedFormat — 8种音频格式 | .mp3/.wav/.flac/.aac/.ogg/.wma/.m4a/.opus | 全部 true |
| TC-012 | IsSupportedFormat — 8种视频格式 | .mp4/.avi/.mov/.wmv/.flv/.mkv/.webm/.m4v | 全部 true |
| TC-013 | IsSupportedFormat — 不支持的格式 | .txt/.jpg/.zip | 全部 false |
| TC-014 | IsSupportedFormat — 大写扩展名 | .MP3/.MP4/.MKV | true（大小写不敏感）|
| TC-017 | DiscoverFilesAsync — 目录不存在 | 不存在的路径 | 返回空序列，不抛异常 |
| TC-018 | DiscoverFilesAsync — 空目录 | 空目录 | 返回空序列 |
| TC-019 | DiscoverFilesAsync — 含媒体文件目录 | 含 .mp3/.mp4 文件的目录 | 返回媒体文件路径，排除 .txt |
| TC-020 | DiscoverFilesAsync — 取消令牌 | 立即取消的 CancellationToken | 抛出 OperationCanceledException |
| — | DiscoverFilesAsync — 递归扫描 | 含子目录的目录 | 找到子目录中的文件 |
| — | DiscoverFilesAsync — 非递归 | recursive=false | 不扫描子目录 |

### TC-R01 ~ TC-R05：真实目录集成测试（FileScannerService）

测试目录：`C:\Users\admin.DESKTOP-4R9CSJI\Documents\xwechat_files\wxid_qbctxev48abj21_23fa\msg\file`  
目录结构：按月份分层（2025-10 ~ 2026-03），递归深度 2 层

| ID | 测试项 | 预期结果 |
|----|--------|----------|
| TC-R01 | 递归扫描真实目录 | 返回非空媒体文件列表 |
| TC-R02 | 递归扫描 — 发现 .mp3 文件 | 至少找到1个 .mp3 文件 |
| TC-R03 | 递归扫描 — 发现 .mp4/.mov 文件 | 至少找到1个视频文件 |
| TC-R04 | 非递归扫描根目录 | 返回空（文件都在子目录中）|
| TC-R05 | 递归扫描 — 过滤非媒体文件 | 不包含 .pdf/.jpg/.png/.docx |

### TC-021 ~ TC-025：DuplicateDetectionService

| ID | 测试项 | 输入 | 预期结果 |
|----|--------|------|----------|
| TC-021 | FindDuplicatesAsync — 无重复 | 所有哈希唯一 | 返回空列表 |
| TC-022 | FindDuplicatesAsync — 一组重复 | 2个文件同哈希 | 返回1个 DuplicateGroup |
| TC-023 | FindDuplicatesAsync — 多组重复 | 3组各2个重复 | 返回3个 DuplicateGroup |
| TC-024 | FindDuplicatesAsync — 文件不存在于仓储 | GetByIdAsync 返回 null | 组内不足2个则排除 |
| TC-025 | FindDuplicatesAsync — 空仓储 | GetAllHashesAsync 返回空 | 返回空列表 |

### TC-031 ~ TC-040：MediaRepository（EF InMemory 集成测试）

| ID | 测试项 | 预期结果 |
|----|--------|----------|
| TC-031 | UpsertAsync — 插入新文件 | 数据库中存在该记录 |
| TC-032 | UpsertAsync — 更新已有文件 | Rating 已更新 |
| TC-033 | GetByIdAsync — 存在的 ID | 返回对应文件 |
| TC-034 | GetByIdAsync — 不存在的 ID | 返回 null |
| TC-035 | GetByPathAsync — 存在的路径 | 返回对应文件 |
| TC-036 | DeleteAsync — 删除文件 | 记录不再存在 |
| TC-037 | SearchAsync — 关键词过滤 | 只返回 FileName 含关键词的文件 |
| TC-038 | SearchAsync — 分页 | Page=2, PageSize=2 返回第3、4条 |
| TC-039 | SearchAsync — 按时长降序排列 | 300s > 180s > 60s |
| TC-040 | GetAllHashesAsync — 排除空哈希 | 只返回非空哈希文件 |
| — | SearchAsync — MediaType 过滤 | 只返回指定类型 |
| — | SearchAsync — 时长范围过滤 | 只返回范围内文件 |
| — | DeleteRangeAsync — 批量删除 | 全部记录被删除 |

---

## 三、执行结果（2026-04-17）

**执行命令**:
```
dotnet test tests\MediaManager.Tests\MediaManager.Tests.csproj --logger "console;verbosity=detailed"
```

| 指标 | 数值 |
|------|------|
| 总用例数 | 64 |
| 通过 | **64** |
| 失败 | 0 |
| 跳过 | 0 |
| 通过率 | **100%** |
| 执行耗时 | 5.6 秒 |
| 运行时 | .NET 8.0.23 |

### 分模块通过情况

| 模块 | 用例数 | 通过 | 失败 |
|------|--------|------|------|
| Core 领域模型 | 11 | 11 | 0 |
| FileScannerService（含真实目录）| 21 | 21 | 0 |
| DuplicateDetectionService | 5 | 5 | 0 |
| MediaRepository | 13 | 13 | 0 |
| 其他补充用例 | 14 | 14 | 0 |

---

## 四、发现的问题

> 详细修改方案见 [issues-2026-04-17.md](issues-2026-04-17.md)

| # | 风险 | 位置 | 问题摘要 |
|---|------|------|----------|
| 1 | 中 | `FfprobeMetadataService:51` | `WaitForExit` 未完成前读取 `ExitCode`，存在异步竞态 |
| 2 | 中 | `FileScannerService:40` | 无权限目录抛出 `UnauthorizedAccessException` 未捕获 |
| 3 | 低 | `DuplicateGroup:11` | `ReclaimableSize` 基于首个文件大小，同组文件大小不一致时计算偏差 |
| 4 | 低 | `MediaRepository:33` | 关键词搜索仅匹配 `FileName`，不覆盖 `Title`/`Artist`/`Album` |

---

## 五、覆盖率概览

| 模块 | 状态 | 说明 |
|------|------|------|
| Core Models | ✅ 已覆盖 | ReclaimableSize, Resolution, ProgressPercent |
| FileScannerService | ✅ 已覆盖 | 含真实目录验证 |
| DuplicateDetectionService | ✅ 已覆盖 | Mock 仓储全路径 |
| MediaRepository | ✅ 已覆盖 | 全部公开方法 |
| HashService | ⚠️ 未覆盖 | 依赖真实文件 I/O，需补充 |
| FfprobeMetadataService | ⚠️ 未覆盖 | 依赖 ffprobe 进程，需补充 |

---

## 六、测试项目结构

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
    │   └── DuplicateDetectionServiceTests.cs
    └── Data/
        └── MediaRepositoryTests.cs
```
