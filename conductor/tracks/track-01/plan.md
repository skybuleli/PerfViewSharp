# Track 01: Project Scaffolding & Core Rendering

## 目标
建立项目的基本骨架，实现一个能在跨平台环境下运行的 Avalonia 窗口，并在其中嵌入 SDL3/Vulkan 渲染表面。

## 任务清单 (Task List)

### 1. 基础设施搭建
- [x] 创建 .NET 解决方案和项目结构。
- [x] 配置 NuGet 引用 (Avalonia, SDL3-CS, TraceEvent)。

### 2. SDL3 + GPU 渲染后端
- [x] 实现 `GpuContext` 初始化逻辑。
- [x] 实现 SDL3 离线渲染 (Offscreen Texture)。
- [x] 实现像素回传 (Readback) 机制。

### 3. Avalonia 集成
- [x] 实现 `SdlView` 控件，通过 `WriteableBitmap` 承载 SDL 渲染内容。
- [x] 确保在 macOS 上能正常运行且不闪退。

## 检查点 (Checkpoints) & 测试案例

### CP1: 解决方案可编译
- **验证方式**：执行 `dotnet build`。
- **预期结果**：编译通过，无错误。

### CP2: 渲染窗口弹出
- **验证方式**：运行 `PerfViewSharp.UI` 项目。
- **预期结果**：弹出一个窗口，内部由 SDL3 驱动显示为纯黑色或指定背景色。
- **测试代码**：在 `PerfViewSharp.Gfx` 中添加一个简单的测试方法，验证 `SDL_CreateWindow` 返回非空指针。

### CP3: 跨平台验证 (macOS)
- **验证方式**：在 macOS 上运行并检查日志。
- **预期结果**：Vulkan 实例成功创建，MoltenVK 加载正常（针对 Apple Silicon）。

## 归档与存档说明
- 每次任务完成后，更新此 plan.md 中的勾选框。
- 关键 Checkpoint 达成后，使用 `git commit` 进行阶段性存档。
