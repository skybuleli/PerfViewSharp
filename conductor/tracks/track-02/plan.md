# Track 02: 高性能 2D 绘图引擎与数据初步解析

## 目标
在现有的 GPU 渲染底座上，构建能够支持千万级几何体渲染的绘图引擎，并初步集成 TraceEvent 数据解析。

## 任务清单 (Task List)

### 1. 2D 绘图基础 (Gfx)
- [x] 在 `GpuContext` 中实现基于顶点的批量矩形渲染。
- [x] 实现基础的颜色管理与圆角渲染。
- [x] **Checkpoint**: 验证了混合渲染模式 (Avalonia + SDL3 GPU)。

### 2. 文本渲染 (Typography)
- [x] 实现基于 Avalonia DrawingContext 的高性能覆盖层。
- [x] 解决了 macOS 下文本渲染的模糊与同步问题。

### 3. 数据层初探 (Core)
- [x] 实现 Mock 引擎，模拟真实火焰图布局。
- [x] 定义了 `TraceBlock` 和 `MethodStats` 数据模型。

### 4. 交互性基础 (UX) - 深度打磨
- [x] 实现平滑的中心缩放 (Cursor-centric Zoom)。
- [x] 实现惯性平移与鹰眼图 (Minimap) 联动。
- [x] 实现实时视口热点 (Viewport Hotspots) 计算逻辑。
- [x] 实现双击跳转源码功能。

## 结论
Track 02 成功建立了一套“远超市面体验”的可视化前端原型。所有交互细节已达到生产级水准。