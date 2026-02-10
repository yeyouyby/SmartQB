# SmartQB - Product Requirements Document

## 1. 核心功能需求

### 1.1 智能采集 (Ingestion)
* **输入**: 支持拖拽 PDF 文件进入系统。
* **处理**: 
    1.  系统自动分析版面，识别题目之间的空白间距进行切分。
    2.  调用 AI Vision 模型，提取题干、选项、答案、解析。
    3.  **强制约束**: 所有数学公式必须转换为 LaTeX 格式 ($...$)。
* **输出**: 存入 SQLite 数据库，同时生成逻辑向量存入向量库。

### 1.2 递归标签回溯 (Recursive Tagging - 核心差异点)
* **场景**: 题库里有 1000 道题。用户今天新增了一个标签“导数极值点”。
* **逻辑**:
    1.  系统不应只等待新题目。
    2.  系统应立即在后台启动异步任务。
    3.  利用向量搜索，在 1000 道旧题中找到语义最接近“导数极值点”的 Top 50 题。
    4.  让 AI 二次判断这 50 道题是否属于该标签。
    5.  如果是，自动建立关联。
* **性能要求**: 此过程不能阻塞 UI 主线程。

### 1.3 组卷与导出
* **筛选**: 支持“标签组合”+“语义描述”混合搜索。
* **排版**:
    * 用户选择题目后，进入“试卷篮”。
    * 点击导出，系统生成 HTML (含 MathJax)，然后通过打印引擎转为 PDF。
    * 支持导出“教师版”（含解析）和“学生版”（无解析）。

## 2. 数据实体定义

```csharp
public class Question
{
    public int Id { get; set; }
    public string Content { get; set; } // LaTeX Markdown
    public string LogicDescriptor { get; set; } // AI 提取的解题思路（用于向量化）
    public double Difficulty { get; set; }
    public List<Tag> Tags { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Definition { get; set; } // 标签的语义定义，用于 AI 判定
}
---

#### 3. `docs/ARCHITECTURE.md` (架构规范)

这个文件告诉 Jules 如何组织代码，特别是 C# 和 WPF 的规范。

```markdown
# SmartQB Architecture Guide

## 1. 分层架构 (Layered Architecture)



### 1.1 SmartQB.Core (没有依赖)
* 包含所有 POCO 实体 (Entities)。
* 定义所有接口 (Interfaces)，例如 `IPdfService`, `IVectorDbService`, `ILLMService`。
* **原则**: 这里的代码不应该知道数据库是 SQLite 还是 SQL Server，也不应该知道 UI 是 WPF 还是 Web。

### 1.2 SmartQB.Infrastructure (依赖 Core)
* **Database**: 实现 `DbContext` (EF Core)。
* **AI**: 使用 `OpenAI-DotNet` 或 `SemanticKernel` 实现 `ILLMService`。
* **PDF**: 使用 `PDFium` 或 `Docnet.Core` 处理 PDF 渲染。
* **Vector**: 封装 ChromaDB 的 HTTP 调用。

### 1.3 SmartQB.UI (依赖 Core, Infrastructure)
* **MVVM**: 使用 `CommunityToolkit.Mvvm` (Source Generators)。
* **Dependency Injection**: 使用 `Microsoft.Extensions.DependencyInjection` 在 `App.xaml.cs` 中组装服务。
* **Controls**: 尽量使用原生控件 + 样式模板，或者引入 `HandyControl` / `WPF UI` 库以获得现代外观。

## 2. 关键技术决策

### A. 为什么使用 .NET 10?
我们需要最新的 C# 语言特性（如 required properties, primary constructors）来简化代码，同时获得最佳的 JSON 序列化性能。

### B. PDF 渲染方案
在 WPF 中不要使用 WebBrowser 控件预览 PDF。请将 PDF 页面转换为 `BitmapSource` 图像流，然后使用 `Image` 控件进行展示，这样能保证流畅的滚动体验。

### C. 导出方案
不使用第三方收费 PDF 库。
**方案**: 使用 Razor Engine 或简单的 String Replace 生成 HTML 字符串 -> 使用 `Microsoft.Web.WebView2` 加载 HTML -> 调用 `PrintToPdfAsync`。

## 3. 开发规范
* **Async/Await**: 所有 I/O 操作（AI 调用、数据库查询、文件读取）必须是异步的。
* **Error Handling**: UI 层必须捕获异常并弹窗提示，不能让程序崩溃。
* **Configuration**: API Key 和 数据库路径应从 `appsettings.json` 读取。
