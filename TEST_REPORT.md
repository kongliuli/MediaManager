# MediaManager 测试报告

**执行时间**: 2026-04-17  
**测试框架**: xUnit 2.8.2 / .NET 8.0.23  
**执行耗时**: 5.6 秒  

---

## 总体结果

| 指标 | 数值 |
|------|------|
| 总用例数 | 64 |
| 通过 | **64** |
| 失败 | 0 |
| 跳过 | 0 |
| 通过率 | **100%** |

---

## 分模块结果

### Core 领域模型（11 个）

| 用例 ID | 测试项 | 结果 |
|---------|--------|------|
| TC-001 | DuplicateGroup.ReclaimableSize — 单文件返回 0 | ✅ 通过 |
| TC-002 | DuplicateGroup.ReclaimableSize — 2个重复返回1份大小 | ✅ 通过 |
| TC-003 | DuplicateGroup.ReclaimableSize — 3个重复返回2份大小 | ✅ 通过 |
| TC-004 | DuplicateGroup.ReclaimableSize — 空列表返回 0 | ✅ 通过 |
| TC-005 | VideoFile.Resolution — 1920×1080 格式化 | ✅ 通过 |
| TC-006 | VideoFile.Resolution — 零值 "0×0" | ✅ 通过 |
| TC-007 | ScanProgressReport.ProgressPercent — 50% 进度 | ✅ 通过 |
| TC-008 | ScanProgressReport.ProgressPercent — 未开始返回 0 | ✅ 通过 |
| TC-009 | ScanProgressReport.ProgressPercent — 完成返回 100 | ✅ 通过 |
| TC-010 | ScanProgressReport.ProgressPercent — 超出上限截断为 100 | ✅ 通过 |
| —      | ScanProgressReport 默认状态为 Idle | ✅ 通过 |

### FileScannerService（21 个）

| 用例 ID | 测试项 | 结果 |
|---------|--------|------|
| TC-011 | IsSupportedFormat — 8种音频格式全部识别 | ✅ 通过 |
| TC-012 | IsSupportedFormat — 8种视频格式全部识别 | ✅ 通过 |
| TC-013 | IsSupportedFormat — .txt/.jpg/.zip 返回 false | ✅ 通过 |
| TC-014 | IsSupportedFormat — 大写扩展名 .MP3/.MP4 不区分大小写 | ✅ 通过 |
| TC-017 | DiscoverFilesAsync — 目录不存在返回空序列 | ✅ 通过 |
| TC-018 | DiscoverFilesAsync — 空目录返回空序列 | ✅ 通过 |
| TC-019 | DiscoverFilesAsync — 含媒体文件目录正确返回 | ✅ 通过 |
| TC-020 | DiscoverFilesAsync — 取消令牌抛出 OperationCanceledException | ✅ 通过 |
| —      | DiscoverFilesAsync — 递归扫描找到子目录文件 | ✅ 通过 |
| —      | DiscoverFilesAsync — 非递归不扫描子目录 | ✅ 通过 |
| TC-R01 | 真实目录递归扫描 — 返回非空媒体文件列表 | ✅ 通过 |
| TC-R02 | 真实目录递归扫描 — 发现 .mp3 文件 | ✅ 通过 |
| TC-R03 | 真实目录递归扫描 — 发现 .mp4/.mov 文件 | ✅ 通过 |
| TC-R04 | 真实目录非递归扫描 — 根目录无直接媒体文件 | ✅ 通过 |
| TC-R05 | 真实目录递归扫描 — 不包含 .pdf/.jpg/.png/.docx | ✅ 通过 |

> 真实测试目录：`C:\Users\admin.DESKTOP-4R9CSJI\Documents\xwechat_files\...\msg\file`  
> 目录结构：按月份分层（2025-10 ~ 2026-03），递归深度 2 层

### DuplicateDetectionService（5 个）

| 用例 ID | 测试项 | 结果 |
|---------|--------|------|
| TC-021 | 所有哈希唯一 — 返回空列表 | ✅ 通过 |
| TC-022 | 一组重复（2个文件）— 返回1个 DuplicateGroup | ✅ 通过 |
| TC-023 | 三组重复 — 返回3个 DuplicateGroup | ✅ 通过 |
| TC-024 | 仓储中文件不存在 — 组内不足2个则排除 | ✅ 通过 |
| TC-025 | 空仓储 — 返回空列表 | ✅ 通过 |

### MediaRepository 集成测试（27 个，EF InMemory）

| 用例 ID | 测试项 | 结果 |
|---------|--------|------|
| TC-031 | UpsertAsync — 插入新文件 | ✅ 通过 |
| TC-032 | UpsertAsync — 相同路径更新已有记录 | ✅ 通过 |
| TC-033 | GetByIdAsync — 存在的 ID 返回文件 | ✅ 通过 |
| TC-034 | GetByIdAsync — 不存在的 ID 返回 null | ✅ 通过 |
| TC-035 | GetByPathAsync — 存在的路径返回文件 | ✅ 通过 |
| TC-036 | DeleteAsync — 删除后记录不再存在 | ✅ 通过 |
| TC-037 | SearchAsync — 关键词过滤只返回匹配文件 | ✅ 通过 |
| TC-038 | SearchAsync — 分页返回正确页数据 | ✅ 通过 |
| TC-039 | SearchAsync — 按时长降序排列 | ✅ 通过 |
| TC-040 | GetAllHashesAsync — 排除空哈希文件 | ✅ 通过 |
| —      | SearchAsync — MediaType 过滤 | ✅ 通过 |
| —      | SearchAsync — 时长范围过滤 | ✅ 通过 |
| —      | DeleteRangeAsync — 批量删除 | ✅ 通过 |

---

## 发现的问题

测试过程中发现以下代码层面的潜在问题（测试通过但值得关注）：

| # | 位置 | 问题描述 | 风险 |
|---|------|----------|------|
| 1 | `DuplicateGroup.ReclaimableSize` | 计算基于 `Files[0].FileSize`，假设所有重复文件大小相同，实际可能因元数据差异略有不同 | 低 |
| 2 | `FfprobeMetadataService` | `process.ExitCode` 在 `WaitForExit` 完成前可能读取不到正确值（异步竞态） | 中 |
| 3 | `MediaRepository.SearchAsync` | 关键词搜索仅匹配 `FileName`，不匹配 `Title`/`Artist` 等元数据字段 | 低 |
| 4 | `FileScannerService` | 目录权限异常（如系统目录）未处理，会向上抛出未捕获异常 | 中 |

---

## 覆盖率概览

| 模块 | 已测试方法 | 未覆盖 |
|------|-----------|--------|
| Core Models | ReclaimableSize, Resolution, ProgressPercent | — |
| FileScannerService | IsSupportedFormat, DiscoverFilesAsync | — |
| HashService | 未测试（需要真实文件 + SHA256 验证） | ComputeAsync |
| DuplicateDetectionService | FindDuplicatesAsync | — |
| MediaRepository | 全部公开方法 | — |
| FfprobeMetadataService | 未测试（依赖 ffprobe 进程） | ExtractAsync, ParseMetadata |
