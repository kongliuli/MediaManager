# MediaManager Web 版本

MediaManager 的 Web 版本，提供媒体文件管理、扫描和重复文件检测的 Web 界面。

## 架构概述

### 技术栈
- **后端**: ASP.NET Core 8 Web API
- **前端**: Blazor Server
- **实时通信**: SignalR
- **数据存储**: SQLite (复用现有数据库)
- **云存储**: 阿里云 OSS (可选)
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
│       │   ├── Layout/          # 页面布局
│       │   ├── Pages/           # 页面组件
│       │   └── FilePreview.razor # 文件预览组件
│       ├── Controllers/          # API 控制器
│       │   ├── MediaController.cs  # 媒体文件管理
│       │   ├── ScanController.cs    # 扫描任务管理
│       │   └── UploadController.cs  # 文件上传管理
│       ├── Services/              # 业务服务
│       │   ├── OssService.cs       # 阿里云 OSS 服务
│       │   └── LocalStorageService.cs # 本地存储服务
│       ├── Hubs/                  # SignalR Hub
│       └── BackgroundServices/     # 后台任务
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
  },
  "Oss": {
    "Enabled": false,
    "AccessKeyId": "your-access-key-id",
    "AccessKeySecret": "your-access-key-secret",
    "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
    "BucketName": "your-bucket-name",
    "PublicUrlPrefix": "https://your-bucket.oss-cn-hangzhou.aliyuncs.com"
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

### 3. 上传管理 (Upload Management) 🆕
- 支持拖拽上传和文件选择上传
- 支持批量上传
- 支持文件预览（图片、音频、视频）
- 自动检测存储方式（本地存储或阿里云OSS）
- 上传进度实时显示
- 上传历史记录

#### 支持的文件格式
**音频格式**:
- MP3, WAV, FLAC, AAC, OGG, M4A

**视频格式**:
- MP4, AVI, MOV, WMV, MKV, WEBM

**图片格式**:
- JPG, JPEG, PNG, GIF, BMP, WEBP

#### 文件大小限制
- 单文件最大: 500MB

### 4. 扫描管理 (Scan Management)
- 创建新的扫描任务
- 实时查看扫描进度
- 支持取消正在运行的任务
- 任务历史记录

### 5. 重复文件 (Duplicates)
- 查找重复文件 (基于文件哈希)
- 可回收空间统计
- 选择保留/删除重复文件

## API 接口

### 上传接口 🆕
- `POST /api/upload` - 上传单个文件
- `POST /api/upload/batch` - 批量上传文件
- `DELETE /api/upload/{filePath}` - 删除上传的文件
- `GET /api/upload/status` - 获取上传状态和配置

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

## 阿里云 OSS 集成 🆕

### 功能特点
- 支持文件上传到阿里云 OSS
- 自动生成预签名 URL 用于访问
- 支持设置文件元数据（Content-Type）
- 支持删除 OSS 中的文件

### 配置步骤
1. 登录阿里云控制台
2. 开通对象存储 OSS 服务
3. 创建存储桶（Bucket）
4. 获取 AccessKey ID 和 AccessKey Secret
5. 在 `appsettings.json` 中配置相关参数
6. 将 `Oss:Enabled` 设置为 `true`

### OSS 配置参数说明
```json
{
  "Oss": {
    "Enabled": true,                    // 是否启用 OSS
    "AccessKeyId": "your-key-id",      // 访问密钥 ID
    "AccessKeySecret": "your-key-secret", // 访问密钥密钥
    "Endpoint": "oss-cn-hangzhou.aliyuncs.com", // OSS 端点
    "BucketName": "media-manager",       // 存储桶名称
    "PublicUrlPrefix": "https://media-manager.oss-cn-hangzhou.aliyuncs.com" // 公共访问前缀
  }
}
```

### 备选方案
当 OSS 未启用或配置不完整时，系统会自动使用本地存储：
- 文件存储在 `wwwroot/uploads/` 目录
- 通过 `/uploads/{filename}` 访问

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

## 文件预览功能 🆕

### 支持的预览类型

#### 图片预览
- 支持格式: JPG, JPEG, PNG, GIF, BMP, WEBP
- 特点: 直接在浏览器中显示

#### 视频预览
- 支持格式: MP4, AVI, MOV, WMV, MKV, WEBM
- 特点: 使用 HTML5 video 标签播放

#### 音频预览
- 支持格式: MP3, WAV, FLAC, AAC, OGG, M4A
- 特点: 使用 HTML5 audio 标签播放

#### 不支持的格式
- 显示提示信息和下载按钮

## 与桌面版本的区别

| 特性 | 桌面版本 (WPF) | Web 版本 |
|-----|----------------|---------|
| UI 技术 | WPF | Blazor Server |
| 部署 | 本地安装 | Web 服务器 |
| 访问方式 | 桌面应用 | 浏览器 |
| 后台任务 | 专用后台线程 | ASP.NET Core Hosted Service |
| 实时通信 | .NET Events | SignalR |
| 文件上传 | 不支持 | 支持 🆕 |
| 云存储 | 不支持 | 支持阿里云 OSS 🆕 |
| 文件预览 | 原生播放器 | Web 原生预览 🆕 |

## 开发说明

### 添加新的 Blazor 页面
1. 在 `Components/Pages/` 中创建新的 `.razor` 文件
2. 添加 `@page "/route"` 路由声明
3. 实现所需的 UI 和逻辑

### 添加新的 API 端点
1. 在 `Controllers/` 中创建新的控制器
2. 添加 HTTP 方法
3. 依赖注入所需的服务

### 扩展存储服务
1. 实现 `IOssService` 接口
2. 在 `Program.cs` 中注册新服务
3. 配置相应的依赖注入

## 注意事项

- Web 版本复用了原有的 Core、Data 和 Services 层
- 数据库格式保持不变，可与桌面版本共享
- FFmpeg 路径需要正确配置，否则缩略图和元数据功能会受影响
- 建议在生产环境中使用更强大的数据库代替 SQLite
- OSS 配置敏感信息建议使用环境变量或密钥管理服务
- 上传文件时需注意安全性，建议添加文件类型验证和大小限制

## 待改进功能

- [ ] 用户认证和权限管理
- [ ] 媒体文件播放功能
- [ ] 更高级的搜索和筛选
- [ ] 批量操作
- [ ] 标签管理
- [ ] 播放列表管理
- [ ] 上传历史持久化存储 🆕
- [ ] 更多云存储提供商支持 🆕
