# PerfViewSharp 技术栈 (Tech Stack)

## 1. 核心框架与运行时
- **运行时**：.NET 9+ (利用 Native AOT)。
- **语言**：C# 13。

## 2. 数据处理引擎
- **追踪解析**：`Microsoft.Diagnostics.Tracing.TraceEvent` (Windows/EventPipe)。
- **Linux 解析**：自研 `perf` 数据解析器 或 绑定 `libbabeltrace2`。
- **存储/索引**：列式存储（DuckDB）或 内存映射文件（MMap）。

## 3. UI 与图形渲染
- **应用壳 (Shell)**：**Avalonia UI** (处理窗口、输入、基础控件)。
- **图形后端**：**SDL3** (通过 `SDL3-CS`)。
- **渲染 API**：**SDL_Gpu** (SDL3 内置的现代 GPU 抽象层，在各平台自动选择 Vulkan/Metal/D3D12)。
- **字体渲染**：SDL_ttf (SDL3 版本) 或 SkiaSharp。

## 4. 辅助技术
- **AI 组件**：Microsoft.Extensions.AI + Local Models。
- **通信**：gRPC (用于远程 Agent 采集)。
- **序列化**：MessagePack 或 Protobuf (高性能序列化)。
