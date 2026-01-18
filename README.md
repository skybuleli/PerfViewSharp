# 🚀 PerfViewSharp

**PerfViewSharp** 是一款高性能、跨平台、现代化的性能分析工具。它旨在彻底重塑 .NET 性能调优体验，让复杂的性能诊断变得直观、高效且愉悦。

![GitHub CI](https://github.com/skybuleli/PerfViewSharp/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

## ✨ 核心亮点

-   **跨平台原生支持**：基于 Avalonia UI + SDL3 GPU，原生支持 Windows, Linux, macOS (Intel & M1)。
-   **GPU 驱动极致渲染**：百万级火焰图块实时缩放，支持 60FPS 平滑交互。
-   **智能分析体验**：
    -   **鹰眼图 (Minimap)**：全局视口掌控。
    -   **关键路径高亮**：自动识别性能瓶颈。
    -   **视口感知热点**：实时计算当前视图内的 Top 耗时函数。
-   **双击跳转源码**：一键从性能瓶颈跳转到 VS Code 等编辑器。

## 🛠️ 技术栈

-   **Runtime**: .NET 9 (with Native AOT)
-   **UI Framework**: Avalonia UI
-   **Graphics**: SDL3 (via SDL3-CS)
-   **Tracing**: Microsoft.Diagnostics.Tracing.TraceEvent

## 🚦 快速开始

### 开发环境要求
- .NET 9.0 SDK
- SDL3 运行时 (`brew install sdl3` on macOS)

### 运行
```bash
git clone https://github.com/skybuleli/PerfViewSharp.git
cd PerfViewSharp
dotnet run --project PerfViewSharp.UI
```

## 🗺️ 路线图 (Roadmap)

- [x] **Track 01**: 基础设施与 GPU 渲染底座打通。
- [x] **Track 02**: 高性能 2D 绘图引擎与交互式 UI。
- [ ] **Track 03**: 海量数据解析与 DuckDB 列式索引集成。
- [ ] **Track 04**: AI 辅助诊断与自动性能报告。

## 📄 开源协议
本项目采用 [MIT License](LICENSE)。
