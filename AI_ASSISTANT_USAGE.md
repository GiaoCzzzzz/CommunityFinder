# AI 课程助手使用说明 / AI Course Assistant Usage Guide

## 功能介绍 / Feature Overview

AI课程助手是一个智能对话界面，允许用户使用自然语言描述他们想要查找的课程，系统会自动解析用户的需求并设置相应的筛选条件。

The AI Course Assistant is an intelligent chat interface that allows users to describe the courses they want to find in natural language. The system automatically parses user requirements and sets corresponding filter conditions.

## 如何使用 / How to Use

### 1. 打开AI助手 / Open AI Assistant

在主页面(MainPage)的右上角，点击 🤖 图标按钮即可打开AI课程助手聊天界面。

On the main page (MainPage), click the 🤖 icon button in the top right corner to open the AI Course Assistant chat interface.

### 2. 输入需求 / Enter Requirements

在聊天界面的输入框中，用自然语言描述你的课程需求。

In the input box of the chat interface, describe your course requirements in natural language.

#### 示例 / Examples:

**中文示例：**
- "我想找周末的运动课程"
- "找一些早上的音乐课"
- "搜索编程相关的课程"
- "周三晚上有什么艺术课？"

**English Examples:**
- "Show me fitness classes on Friday"
- "I want to learn dancing in the afternoon"
- "Find programming courses"
- "Music classes on Monday morning"

### 3. AI解析和响应 / AI Parsing and Response

AI助手会立即分析你的输入，提取出以下信息：

The AI assistant will immediately analyze your input and extract the following information:

- **分类建议 / Category Suggestions**: 根据关键词推荐课程分类
- **上课日期 / Class Day**: 星期几（Monday-Sunday）
- **时间段 / Time Slot**: 早上/下午/晚上 (Morning/Afternoon/Evening)
- **搜索关键词 / Search Keywords**: 提取的相关搜索词

### 4. 应用筛选 / Apply Filters

如果AI成功识别出筛选条件，会显示"应用筛选并返回"按钮。点击后：

If the AI successfully identifies filter conditions, an "Apply and Return" button will appear. After clicking:

1. 筛选条件会自动应用到主页面
2. 系统返回到课程列表页面
3. 课程列表会根据新的筛选条件自动更新

## 技术实现 / Technical Implementation

### 核心组件 / Core Components

#### 1. AICourseAssistantService
- 位置：`Services/AICourseAssistantService.cs`
- 功能：自然语言处理和参数提取
- Features: Natural language processing and parameter extraction

#### 2. AIChatPage
- 位置：`Views/AIChatPage.xaml` 和 `Views/AIChatPage.xaml.cs`
- 功能：聊天界面UI和交互逻辑
- Features: Chat interface UI and interaction logic

#### 3. MainPage Integration
- 在顶部导航栏添加了AI助手按钮 🤖
- Added AI assistant button 🤖 to the top navigation bar

### 支持的关键词 / Supported Keywords

#### 分类关键词 / Category Keywords
- Education & Enrichment: education, learning, study, school, 教育
- Sports & Fitness: sports, fitness, exercise, gym, workout, 运动, 健身
- Arts & Crafts: art, craft, painting, drawing, 艺术, 手工
- Music & Dance: music, dance, singing, 音乐, 舞蹈
- Technology: technology, tech, coding, programming, computer, 科技, 编程

#### 星期关键词 / Day Keywords
- Monday/周一/星期一, Tuesday/周二/星期二, etc.
- weekend/周末

#### 时间段关键词 / Time Slot Keywords
- Morning: morning, 早上, 上午
- Afternoon: afternoon, 下午
- Evening: evening, 晚上, night

## 未来改进 / Future Improvements

1. **集成真实AI模型** / **Integrate Real AI Model**
   - 当前实现使用基于规则的关键词匹配
   - 可以集成OpenAI GPT、Google Gemini等真实AI模型
   - Current implementation uses rule-based keyword matching
   - Can integrate real AI models like OpenAI GPT, Google Gemini

2. **上下文理解** / **Context Understanding**
   - 支持多轮对话
   - 记住用户偏好
   - Support multi-turn conversations
   - Remember user preferences

3. **智能推荐** / **Intelligent Recommendations**
   - 基于历史浏览记录推荐课程
   - 个性化课程建议
   - Recommend courses based on browsing history
   - Personalized course suggestions

4. **语音输入** / **Voice Input**
   - 支持语音识别输入
   - Support voice recognition input

## 代码示例 / Code Examples

### 使用AI服务解析查询 / Using AI Service to Parse Queries

```csharp
var aiService = new AICourseAssistantService();
var filterParams = aiService.ParseQuery("我想找周末的运动课程");

// filterParams.Day = "Saturday"
// filterParams.SuggestedCategory = "Sports & Fitness"
// filterParams.AIResponse = "✨ 我已经理解您的需求：..."
```

### 应用筛选到ViewModel / Apply Filters to ViewModel

```csharp
if (!string.IsNullOrWhiteSpace(filterParams.Day))
{
    coursesViewModel.SelectedDay = filterParams.Day;
}

if (!string.IsNullOrWhiteSpace(filterParams.Time))
{
    coursesViewModel.SelectedTime = filterParams.Time;
}

if (!string.IsNullOrWhiteSpace(filterParams.SearchText))
{
    coursesViewModel.SearchText = filterParams.SearchText;
}
```

## 问题排查 / Troubleshooting

### AI无法理解我的输入 / AI Cannot Understand My Input

1. 尝试使用更具体的描述
2. 参考示例查询格式
3. 分别描述不同的筛选条件

### 筛选条件没有正确应用 / Filter Conditions Not Applied Correctly

1. 确保点击了"应用筛选并返回"按钮
2. 检查主页面的筛选器是否已更新
3. 尝试手动点击"搜索"按钮

## 反馈与建议 / Feedback and Suggestions

如果您有任何问题或建议，请在GitHub仓库中提交Issue。

If you have any questions or suggestions, please submit an Issue in the GitHub repository.

---

**版本 / Version**: 1.0.0  
**最后更新 / Last Updated**: 2025-10-12
