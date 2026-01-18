# PerfViewSharp 产品定义 (Product Definition)

## 1. 愿景与目标
**PerfViewSharp** 是一款高性能、跨平台、现代化的性能分析工具。它旨在解决原版 PerfView 界面陈旧、仅限 Windows、交互困难等痛点，同时保留其处理海量追踪数据的核心能力。

## 2. 核心功能 (Core Features)
- **跨平台支持**：原生支持 Windows (ETW), Linux (perf/eBPF/EventPipe), macOS (Instruments/EventPipe)。
- **高性能可视化**：基于 GPU 加速的火焰图、调用树、时间轴，支持千万级数据点的流畅交互。
- **现代化 UX**：
  - 动态过滤与搜索（DSL 驱动）。
  - 多任务/分屏工作流。
  - AI 辅助分析（热点发现、死锁检测）。
- **实时与离线分析**：既能解析历史追踪文件，也能实时连接到运行中的进程。

## 3. 目标用户
- .NET 性能调优专家。
- 系统架构师与 SRE。
- 需要在 Linux 环境分析 .NET 程序的开发者。

## 4. 核心差异化竞争点
1. **GPU 渲染引擎**：SDL3 + Vulkan 实现极致缩放。
2. **列式存储引擎**：DuckDB/MMap 实现大文件即时过滤。
3. **AI 诊断**：集成本地模型自动生成调优报告。
