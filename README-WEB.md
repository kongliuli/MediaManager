# MediaManager Web 版本

MediaManager 的 Web 版本，提供媒体文件管理、扫描和重复文件检测的 Web 界面。

## 架构概述

### 技术栈
- **后端**: ASP.NET Core 8 Web API
- **前端**: Blazor Server
- **实时通信**: SignalR
- **数据存储**: SQLite (复用现有数据库)
- **日志**: Serilog

### 项目结构
```
MediaManager/
├── src/
│   ├── MediaManager.Core/       # 核心领域模型和接口
│   ├── MediaManager.Data/       # 数据访问层
│   ├── MediaManager.Services/   # 业务逻辑层
│   ├── MediaManager.UI/         # WPF UI (原始桌面版本)
│   └── MediaManager.Api/        # Web API 和 Blazor UI (新增)
│       ├── Components/
│       │   ├── Layout/         # 页面布局
│       │   └── Pages/          # 页面组件
│       ├── Controllers/        # API 控制器
│       ├── Hubs/              # SignalR Hub
│       └── BackgroundServices/ # 后台任务
└── tests/
```

## 快速开始

### 前置条件
- .NET 8 SDK
- FFmpeg/FFprobe (用于媒体处理)

### 配置

1. 配置 `appsettings.json`：
```json
{
  "AppConfig": {
    "DatabasePath": "../media.db",
    "ThumbnailDirectory": "../thumbnails",
    "FfmpegPath": "path/to/ffmpeg",
    "FfprobePath": "path/to/ffprobe"
  }
}
```

2. 运行项目：
```bash
cd src/MediaManager.Api
dotnet run
```

3. 访问 Web 界面：
- 默认地址: `https://localhost:5001` (或配置的地址)
- Swagger 文档: `https://localhost:5001/swagger`

## 功能特性

### 1. 首页 (Dashboard)
- 媒体库统计概览
- 最近扫描任务列表
- 快捷操作按钮

### 2. 媒体库 (Media Library)
- 浏览和搜索媒体文件
- 按类型筛选
- 分页显示
- 查看文件详情
- 删除文件

### 3. 扫描管理 (Scan Management)
- 创建新的扫描任务
- 实时查看扫描进度
- 支持取消正在运行的任务
- 任务历史记录

### 4. 重复文件 (Duplicates)
- 查找重复文件 (基于文件哈希)
- 可回收空间统计
- 选择保留/删除重复文件

## API 接口

### 扫描接口
- `POST /api/scan/start` - 启动新扫描
- `GET /api/scan/tasks` - 获取所有任务
- `GET /api/scan/tasks/{id}` - 获取任务详情
- `POST /api/scan/tasks/{id}/cancel` - 取消任务

### 媒体接口
- `GET /api/media` - 获取媒体列表
- `GET /api/media/{id}` - 获取媒体详情
- `PUT /api/media/{id}` - 更新媒体信息
- `DELETE /api/media/{id}` - 删除媒体
- `GET /api/media/stats` - 获取统计信息
- `GET /api/media/{id}/thumbnail` - 获取缩略图

## SignalR 实时通信

### 扫描进度更新
```csharp
// 客户端接收
ReceiveProgressUpdate: {
  taskId: string,
  status: string,
  progressPercent: number,
  currentFile: string,
  processedCount: number,
  totalCount: number,
  message: string
}
```

### 扫描完成通知
```csharp
// 客户端接收
ReceiveScanCompleted: {
  taskId: string,
  success: boolean,
  totalFiles: number,
  newFiles: number,
  updatedFiles: number,
  skippedFiles: number,
  duration: TimeSpan,
  message: string,
  errors: string[]
}
```

## 与桌面版本的区别

| 特性 | 桌面版本 (WPF) | Web 版本 |
|-----|----------------|---------|
| UI 技术 | WPF | Blazor Server |
| 部署 | 本地安装 | Web 服务器 |
| 访问方式 | 桌面应用 | 浏览器 |
| 后台任务 | 专用后台线程 | ASP.NET Core Hosted Service |
| 实时通信 | .NET Events | SignalR |

## 开发说明

### 添加新的 Blazor 页面
1. 在 `Components/Pages/` 中创建新的 `.razor` 文件
2. 添加 `@page "/route"` 路由声明
3. 实现所需的 UI 和逻辑

### 添加新的 API 端点
1. 在 `Controllers/` 中创建新的控制器
2. 添加 HTTP 方法
3. 依赖注入所需的服务

## 注意事项

- Web 版本复用了原有的 Core、Data 和 Services 层
- 数据库格式保持不变，可与桌面版本共享
- FFmpeg 路径需要正确配置，否则缩略图和元数据功能会受影响
- 建议在生产环境中使用更强大的数据库代替 SQLite

## 待改进功能

- [ ] 用户认证和权限管理
- [ ] 媒体文件播放功能
- [ ] 更高级的搜索和筛选
- [ ] 批量操作
- [ ] 标签管理
- [ ] 播放列表管理
