# SmartQB (Intelligent Question Bank)

> **Current Status**: Initialization / Architecture Definition
> **Target Framework**: .NET 10
> **UI Framework**: WPF (Windows Presentation Foundation)

## 📖 项目简介
SmartQB 是一个基于本地桌面的智能题库管理系统。它利用 AI (VLM) 视觉大模型将非结构化的 PDF 试卷转化为结构化的数据资产，并支持基于语义的搜索和自动化的标签管理。

我们的目标是解决传统题库“存而不活”的问题，通过 AI 赋予题目“逻辑特征”，实现从“搜关键词”到“搜解题思路”的跨越。

## 🏗️ 技术栈 (Tech Stack)

* **Runtime**: .NET 10 (C#)
* **Frontend**: WPF (使用 `CommunityToolkit.Mvvm` 进行 MVVM 绑定)
* **Database**: 
    * Metadata: SQLite (Entity Framework Core)
    * Vectors: ChromaDB (or local vector search via Semantic Kernel)
* **AI Integration**: OpenAI Compatible API (GPT-4o / DeepSeek / Ollama)
* **PDF Engine**: Docnet 或 PDFium (渲染), SkiaSharp (图像处理)

## 🚀 给 Jules 的开发指南 (Task List)

Jules，请按照以下顺序实现系统。请确保遵循 `src/` 下的分层架构。

### Phase 1: 基础设施 (Infrastructure)
- [ ] 创建 .NET 10 Solution，包含 Core, Infrastructure, UI 三个项目。
- [ ] 在 `Infrastructure` 中实现 `IAIService`，封装 OpenAI 兼容的 API 调用。
- [ ] 在 `Infrastructure` 中实现 PDF 解析服务，支持将 PDF 页面转为高 DPI 图片。
- [ ] 配置 EF Core + SQLite，建立 `Question` 和 `Tag` 的实体关系。

### Phase 2: 核心业务 (Core Domain)
- [ ] 实现 **Ingestion Pipeline**：PDF -> 切图 -> AI OCR -> 结构化实体。
- [ ] 实现 **Recursive Tagging Service**：这是核心难点。当添加新标签时，需逻辑回溯旧题库。
- [ ] 实现 **Vector Service**：处理题目的 `LogicDescription` 嵌入与检索。

### Phase 3: 界面交互 (WPF UI)
- [ ] **MainLayout**: 侧边栏导航 + 内容区域。
- [ ] **ImportView**: 拖拽上传 PDF，实时显示处理进度的进度条 (Async/Await)。
- [ ] **LibraryView**: `CollectionView` 展示题目列表，支持多选和右侧预览。
- [ ] **ExportView**: 使用 HTML 模板 + `WebView2` 打印功能生成 PDF。

## 📂 文档导航
* [产品需求文档 (PRD)](docs/PRD.md) - 了解业务细节
* [架构设计 (Architecture)](docs/ARCHITECTURE.md) - 了解代码组织方式
