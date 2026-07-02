# MediaManager

音视频浏览与媒体库管理项目。

## 定位

`MediaManager` 用于扫描、索引和管理本地媒体文件，包含媒体识别、搜索、重复检测、缩略图/元数据处理、播放列表、标签、上传和后台扫描等能力。

## 项目结构

| 路径 | 用途 |
| --- | --- |
| `src/MediaManager.Core` | 领域模型、接口、媒体类型、扫描/搜索相关抽象 |
| `src/MediaManager.Data` | EF Core 数据访问、仓储、迁移 |
| `src/MediaManager.Services` | 扫描、哈希、重复检测、缩略图、元数据等服务实现 |
| `src/MediaManager.Api` | Web/API 入口、Blazor 页面、后台扫描服务 |
| `tests` | xUnit 测试 |

## 验证记录

历史测试报告见 `TEST_REPORT.md`，当时结果为 64 个用例全部通过。

## 分支说明

`main` 是当前默认主线。

`origin/trae/solo-agent-rLpN6W` 包含 API、后台扫描、缓存、性能组件和 Blazor 页面等实现，后续需要单独验证后再决定是否合入 `main`。
