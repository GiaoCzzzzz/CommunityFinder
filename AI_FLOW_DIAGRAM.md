# AI Course Assistant Flow Diagram

## Overall Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         MainPage                            │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Navigation Bar                                     │    │
│  │  [Logo] [Tabs] [🤖 AI] [⚙️ Settings]              │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Course Filters (CoursesViewModel)                  │    │
│  │  • L1, L2, L3 Category Selectors                    │    │
│  │  • Day Picker                                        │    │
│  │  • Time Picker                                       │    │
│  │  • Search Text                                       │    │
│  │  • Where Picker                                      │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Course List (CollectionView)                       │    │
│  │  [Course Item 1]                                     │    │
│  │  [Course Item 2]                                     │    │
│  │  [Course Item 3]                                     │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                          │
                          │ User clicks 🤖 button
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                      AIChatPage                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  🤖 AI 课程助手                                     │    │
│  │  告诉我你想找什么样的课程，我会帮你自动筛选！        │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Chat History                                        │    │
│  │  ┌────────────────────────────────────────┐         │    │
│  │  │ 👋 Welcome message with examples       │         │    │
│  │  └────────────────────────────────────────┘         │    │
│  │                                                      │    │
│  │  ┌────────────────────┐                             │    │
│  │  │ User: 我想找周末的  │ (User Message - Right)     │    │
│  │  │      运动课程       │                             │    │
│  │  └────────────────────┘                             │    │
│  │                                                      │    │
│  │  ┌────────────────────┐                             │    │
│  │  │ AI: ✨ 我已经理解   │ (AI Response - Left)       │    │
│  │  │ 您的需求：          │                             │    │
│  │  │ 📅 Day: Saturday   │                             │    │
│  │  │ 📚 Category: Sports│                             │    │
│  │  └────────────────────┘                             │    │
│  │                                                      │    │
│  │  ┌─────────────────────────┐                        │    │
│  │  │ [✓ 应用筛选并返回]      │ (Apply Button)        │    │
│  │  └─────────────────────────┘                        │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Input Area                                          │    │
│  │  [输入你的需求...]               [发送]            │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

```
User Input
    │
    ▼
┌───────────────────────────────────────────────────┐
│         AICourseAssistantService                  │
│                                                   │
│  1. ParseQuery(userQuery)                        │
│     ├─→ Extract Category Keywords                │
│     ├─→ Extract Day Keywords                     │
│     ├─→ Extract Time Keywords                    │
│     └─→ Extract Search Keywords                  │
│                                                   │
│  2. GenerateResponse(filterParams)               │
│     └─→ Create formatted AI response             │
│                                                   │
│  3. Return CourseFilterParams                    │
│     ├─ SuggestedCategory                         │
│     ├─ Day                                        │
│     ├─ Time                                       │
│     ├─ SearchText                                 │
│     ├─ Where                                      │
│     └─ AIResponse                                 │
└───────────────────────────────────────────────────┘
    │
    ▼
Display in AIChatPage
    │
    ▼
User clicks "Apply"
    │
    ▼
┌───────────────────────────────────────────────────┐
│         ApplyFiltersToViewModel()                 │
│                                                   │
│  coursesViewModel.SelectedDay = filterParams.Day │
│  coursesViewModel.SelectedTime = filterParams.Time│
│  coursesViewModel.SearchText = filterParams.SearchText│
│                                                   │
└───────────────────────────────────────────────────┘
    │
    ▼
Navigate back to MainPage
    │
    ▼
Filters are applied
    │
    ▼
User clicks Search button
    │
    ▼
Course list updates with filtered results
```

## Keyword Matching Logic

```
Input: "我想找周末的运动课程"
         ↓
    ┌────────────────────────┐
    │  Tokenization          │
    │  ["我", "想", "找",    │
    │   "周末", "的",        │
    │   "运动", "课程"]      │
    └────────────────────────┘
         ↓
    ┌────────────────────────────────────┐
    │  Keyword Matching                  │
    │  • "周末" → Day: "Saturday"        │
    │  • "运动" → Category: "Sports"     │
    └────────────────────────────────────┘
         ↓
    ┌────────────────────────┐
    │  CourseFilterParams    │
    │  Day: "Saturday"       │
    │  Category: "Sports"    │
    └────────────────────────┘
```

## User Interaction Flow

```
Step 1: User opens MainPage
          ↓
Step 2: User clicks 🤖 AI button
          ↓
Step 3: AIChatPage opens with welcome message
          ↓
Step 4: User types query in input box
          ↓
Step 5: User presses Enter or clicks "发送"
          ↓
Step 6: User message appears on right side
          ↓
Step 7: AI processes query (instant, <100ms)
          ↓
Step 8: AI response appears on left side
          ↓
Step 9: If filters detected, "Apply" button appears
          ↓
Step 10: User clicks "Apply" button
          ↓
Step 11: Success message shown
          ↓
Step 12: Navigate back to MainPage
          ↓
Step 13: Filters are now applied in UI
          ↓
Step 14: User clicks Search button
          ↓
Step 15: Filtered course results displayed
```

## Component Relationships

```
┌─────────────────────────────────────────────────────────┐
│                        App.xaml.cs                      │
│                                                         │
│  ┌───────────────────────────────────────────────┐    │
│  │  MainPage.xaml.cs                             │    │
│  │  • CoursesViewModel _vm                       │    │
│  │  • OnAIChatClicked() handler                  │    │
│  │    └─→ Navigation.PushAsync(new AIChatPage()) │    │
│  └───────────────────────────────────────────────┘    │
│                         │                               │
│                         │ passes _vm                    │
│                         ▼                               │
│  ┌───────────────────────────────────────────────┐    │
│  │  AIChatPage.xaml.cs                           │    │
│  │  • AICourseAssistantService _aiService        │    │
│  │  • CoursesViewModel _coursesViewModel         │    │
│  │  • OnSendClicked() handler                    │    │
│  │    └─→ _aiService.ParseQuery()                │    │
│  │  • OnApplyFiltersClicked() handler            │    │
│  │    └─→ ApplyFiltersToViewModel()              │    │
│  └───────────────────────────────────────────────┘    │
│                         │                               │
│                         │ uses                          │
│                         ▼                               │
│  ┌───────────────────────────────────────────────┐    │
│  │  AICourseAssistantService.cs                  │    │
│  │  • ParseQuery(string) → CourseFilterParams    │    │
│  │  • GenerateResponse() → string                │    │
│  │  • GetExamples() → List<string>               │    │
│  └───────────────────────────────────────────────┘    │
│                         │                               │
│                         │ returns                       │
│                         ▼                               │
│  ┌───────────────────────────────────────────────┐    │
│  │  CourseFilterParams (Model)                   │    │
│  │  • SuggestedCategory                          │    │
│  │  • Day                                        │    │
│  │  • Time                                       │    │
│  │  • SearchText                                 │    │
│  │  • Where                                      │    │
│  │  • AIResponse                                 │    │
│  └───────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

## Future Enhancement: Real AI Integration

```
Current Implementation:
┌──────────────┐     ┌─────────────────────────┐
│ User Query   │────→│ Rule-Based Keyword      │
│              │     │ Matching                │
└──────────────┘     └─────────────────────────┘
                              │
                              ▼
                     ┌─────────────────┐
                     │ Filter Params   │
                     └─────────────────┘

Future Implementation:
┌──────────────┐     ┌─────────────────────────┐
│ User Query   │────→│ OpenAI GPT / Gemini     │
│              │     │ Natural Language        │
│              │     │ Understanding           │
└──────────────┘     └─────────────────────────┘
                              │
                              ▼
                     ┌─────────────────────────┐
                     │ Context-Aware Response  │
                     │ • Intent Recognition    │
                     │ • Multi-turn Dialog     │
                     │ • Personalization       │
                     └─────────────────────────┘
                              │
                              ▼
                     ┌─────────────────┐
                     │ Filter Params   │
                     └─────────────────┘
```
